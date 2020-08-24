using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using GiftCardPaymentProvider;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class PaymentDemoController : Controller
    {
        private ReferenceConverter _referenceConverter;
        private IContentLoader _contentLoader;
        private AssetUrlResolver _assetUrlResolver;
        private ICurrentMarket _currentMarket;
        private IPaymentProcessor _paymentProcessor;
        private IOrderRepository _orderRepository;
        private IOrderGroupFactory _orderGroupFactory;
        private IOrderGroupCalculator _orderGroupCalculator;

        public PaymentDemoController(ReferenceConverter referenceConverter, IContentLoader contentLoader,
            AssetUrlResolver assetUrlResolver, ICurrentMarket currentMarket, IPaymentProcessor paymentProcessor,
            IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory, IOrderGroupCalculator orderGroupCalculator)
        {
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _assetUrlResolver = assetUrlResolver;
            _currentMarket = currentMarket;
            _paymentProcessor = paymentProcessor;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
        }
        public ActionResult Index()
        {
            var viewModel = new PaymentDemoViewModel();
            InitializeModel(viewModel);

            return View(viewModel);
        }

        private void InitializeModel(PaymentDemoViewModel viewModel)
        {
            ICart cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            var suspendersRef = _referenceConverter.GetContentLink("Suspenders_1");

            if(viewModel.Variants == null)
            {
                viewModel.Variants = new List<DefaultVariation>
                {
                    _contentLoader.Get<DefaultVariation>(shirtRef),
                    _contentLoader.Get<DefaultVariation>(suspendersRef)
                };
            }

            viewModel.PayMethods = PaymentManager.GetPaymentMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value).PaymentMethod;
            viewModel.CartItems = cart.GetAllLineItems();
            viewModel.CartTotal = cart.GetTotal();

            viewModel.GiftCards = GiftCardService.GetClientGiftCards("TrainingGiftCard", (PrimaryKeyId)CustomerContext.Current.CurrentContactId);
            viewModel.ShippingMethods = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false).ShippingMethod.ToList();
        }

        public ActionResult UpdateCart(int PurchaseQuantity, string itemCode)
        {
            var itemRef = _referenceConverter.GetContentLink(itemCode);
            var itemVar = _contentLoader.Get<DefaultVariation>(itemRef);

            var cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");
            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == itemCode);

            if(lineItem == null)
            {
                lineItem = _orderGroupFactory.CreateLineItem(itemCode, cart);
                lineItem.Quantity = PurchaseQuantity;
                lineItem.PlacedPrice = itemVar.GetDefaultPrice().UnitPrice;
                if (itemVar.RequireSpecialShipping)
                {
                    IShipment specialShip = _orderGroupFactory.CreateShipment(cart);
                    specialShip.ShippingMethodId = GetShipMethodByParam(itemCode);
                    specialShip.ShippingAddress = GetOrderAddress(cart);
                    cart.AddShipment(specialShip);
                    cart.AddLineItem(specialShip, lineItem);
                }
                else
                {
                    var ship = cart.GetFirstShipment();
                    ship.ShippingAddress = GetOrderAddress(cart);

                    cart.AddLineItem(lineItem);
                }               
            }
            else
            {
                var shipment = cart.GetFirstForm().Shipments.
                    Where(s => s.LineItems.Contains(lineItem) == true).FirstOrDefault();

                cart.UpdateLineItemQuantity(shipment, lineItem, PurchaseQuantity);
            }
            
            _orderRepository.Save(cart);

            return RedirectToAction("Index");
        }

        private Guid GetShipMethodByParam(string paramCodeValue)
        {
            var paramRow = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false).
                ShippingMethodParameter.Where(p => p.Value == paramCodeValue).FirstOrDefault();
            return paramRow.ShippingMethodId;
        }

        private Guid GetSipMethodByOptionParam(string itemCode)
        {
            string paramName = itemCode.Split('_')[0];
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false);
            Guid optionParam = dto.ShippingOptionParameter.Where(r => r.Parameter.Contains(paramName)).FirstOrDefault().ShippingOptionId;
            return dto.ShippingMethod.Where(r => r.ShippingOptionId == optionParam).FirstOrDefault().ShippingMethodId;
        }

        private IOrderAddress GetOrderAddress(IOrderGroup cart)
        {
            var shipAddress = _orderGroupFactory.CreateOrderAddress(cart);
            shipAddress.City = "Atlanta";
            shipAddress.CountryCode = "USA";
            shipAddress.CountryName = "United States";
            shipAddress.Id = "DemoShipAddress";

            return shipAddress;
        }

        public ActionResult SimulatePurchase(PaymentDemoViewModel viewModel)
        {
            var cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");
            cart.GetFirstShipment().ShippingMethodId = viewModel.SelectedShippingMethodId;

            var primaryPayment = _orderGroupFactory.CreatePayment(cart);
            primaryPayment.PaymentMethodId = viewModel.SelectedPaymentId;
            primaryPayment.Amount = _orderGroupCalculator.GetTotal(cart).Amount;
            primaryPayment.PaymentMethodName = PaymentManager.GetPaymentMethod(viewModel.SelectedPaymentId).PaymentMethod[0].Name;

            if (viewModel.UseGiftCard)
            {
                var giftMethod = PaymentManager.GetPaymentMethodBySystemName("GiftCard", ContentLanguage.PreferredCulture.Name);
                var giftPayment = _orderGroupFactory.CreatePayment(cart);
                giftPayment.PaymentMethodId = giftMethod.PaymentMethod[0].PaymentMethodId;
                giftPayment.Amount = viewModel.GiftCardDebitAmt;
                giftPayment.ValidationCode = viewModel.RedemtionCode;
                giftPayment.PaymentMethodName = giftMethod.PaymentMethod[0].Name;
                
                PaymentProcessingResult giftPayResult = _paymentProcessor.ProcessPayment(cart, giftPayment, cart.GetFirstShipment());
                if (giftPayResult.IsSuccessful)
                {
                    primaryPayment.Amount -= giftPayment.Amount;
                    cart.AddPayment(giftPayment);
                }
                viewModel.GiftInfoMessage = giftPayResult.Message;
            }

            PaymentProcessingResult payResult = _paymentProcessor.ProcessPayment(cart, primaryPayment, cart.GetFirstShipment());

            if (payResult.IsSuccessful)
            {
                cart.AddPayment(primaryPayment);
                _orderRepository.SaveAsPurchaseOrder(cart);
                _orderRepository.Delete(cart.OrderLink);
            }
            viewModel.MessageOutput = payResult.Message;

            InitializeModel(viewModel);    

            return View("Index", viewModel);
        }
    }

    public static class CustomHelpers
    {
        public static IHtmlString UrlResolver(this HtmlHelper helper, IAssetContainer asset)
        {
            var resolver = ServiceLocator.Current.GetInstance<AssetUrlResolver>();
            var Url = resolver.GetAssetUrl(asset);
            return new MvcHtmlString(Url);
        }
    }
}
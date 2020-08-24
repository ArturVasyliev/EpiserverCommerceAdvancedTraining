using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class MarketsDemoController : Controller
    {
        private IMarketService _marketService;
        private ICurrentMarket _currentMarket;
        private ReferenceConverter _referenceConverter;
        private IContentLoader _contentLoader;
        private IOrderRepository _orderRepository;
        private IOrderGroupFactory _orderGroupFactory;
        private ITaxCalculator _taxCalculator;
        private IPriceService _priceService;
        private IPromotionEngine _promotionEngine;

        public MarketsDemoController(IMarketService marketService, ICurrentMarket currentMarket,
            ReferenceConverter referenceConverter, IContentLoader contentLoader,
            IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory,
            ITaxCalculator taxCalculator, IPriceService priceService,
            IPromotionEngine promotionEngine)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _taxCalculator = taxCalculator;
            _priceService = priceService;
            _promotionEngine = promotionEngine;
        }
        // GET: MarketsDemo
        public ActionResult Index()
        {
            var viewModel = new DemoMarketsViewModel();
            viewModel.MarketList = _marketService.GetAllMarkets();
            viewModel.SelectedMarket = _currentMarket.GetCurrentMarket();

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            viewModel.Shirt = _contentLoader.Get<ShirtVariation>(shirtRef);

            GetTaxInfo(viewModel);
            GetPriceInfo(viewModel);

            return View(viewModel);
        }

        public ActionResult ChangeDefaultMarket(string selectedMarket)
        {
            if (selectedMarket != null)
            {
                _currentMarket.SetCurrentMarket(new MarketId(selectedMarket));
            }
            var viewModel = new DemoMarketsViewModel();
            viewModel.MarketList = _marketService.GetAllMarkets();
            viewModel.SelectedMarket = _currentMarket.GetCurrentMarket();

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            viewModel.Shirt = _contentLoader.Get<ShirtVariation>(shirtRef);

            GetTaxInfo(viewModel);
            GetPriceInfo(viewModel);

            return View("Index", viewModel);
        }

        private void GetTaxInfo(DemoMarketsViewModel viewModel)
        {
            ICart cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "BogusCart");

            IOrderAddress bogusAddress = _orderGroupFactory.CreateOrderAddress(cart);
            bogusAddress.CountryCode = viewModel.SelectedMarket.Countries.FirstOrDefault();
            bogusAddress.City = "Stockholm";
            viewModel.TaxAmountOldSchool = GetTaxOldSchool(viewModel, bogusAddress);

            ILineItem lineItem = _orderGroupFactory.CreateLineItem(viewModel.Shirt.Code, cart);
            lineItem.Quantity = 1;
            lineItem.PlacedPrice = viewModel.Shirt.GetDefaultPrice().UnitPrice;
            lineItem.TaxCategoryId = viewModel.Shirt.TaxCategoryId;

            cart.AddLineItem(lineItem);

            viewModel.TaxAmount = _taxCalculator.GetSalesTax(lineItem, viewModel.SelectedMarket,
                bogusAddress, new Money(lineItem.PlacedPrice, viewModel.SelectedMarket.DefaultCurrency));
        }

        private Money GetTaxOldSchool(DemoMarketsViewModel viewModel, IOrderAddress orderAddress)
        {
            string taxCategory = CatalogTaxManager.GetTaxCategoryNameById((int)viewModel.Shirt.TaxCategoryId);

            viewModel.Taxes = OrderContext.Current.GetTaxes(Guid.Empty,
                taxCategory, viewModel.SelectedMarket.DefaultLanguage.TwoLetterISOLanguageName,  orderAddress);

            decimal decTaxTotal = (decimal)(from x in viewModel.Taxes
                                    where x.TaxType == TaxType.SalesTax
                                    select x).Sum((ITaxValue x) => x.Percentage);

            decimal itemPrice = viewModel.Shirt.GetDefaultPrice().UnitPrice;

            return new Money(itemPrice * decTaxTotal / 100m, viewModel.SelectedMarket.DefaultCurrency);
        }

        private void GetPriceInfo(DemoMarketsViewModel viewModel)
        {
            var filter = new PriceFilter();

            viewModel.OptimizedPrices = _priceService.GetPrices(viewModel.SelectedMarket.MarketId,
                DateTime.Now, new CatalogKey(viewModel.Shirt.Code), filter);

            filter.Quantity = 1;
            var custPricing = new List<CustomerPricing>
            { new CustomerPricing(CustomerPricing.PriceType.AllCustomers, string.Empty) };


            if (CustomerContext.Current.CurrentContact != null)
            {
                if (!string.IsNullOrEmpty(PrincipalInfo.Current.Name))
                {
                    custPricing.Add(new CustomerPricing(CustomerPricing.PriceType.UserName,
                        PrincipalInfo.Current.Name));
                }

                if (!string.IsNullOrEmpty(CustomerContext.Current.CurrentContact.EffectiveCustomerGroup))
                {
                    custPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup,
                        CustomerContext.Current.CurrentContact.EffectiveCustomerGroup));
                }
            }

            filter.CustomerPricing = custPricing;

            viewModel.FilteredPrices = _priceService.GetPrices(viewModel.SelectedMarket.MarketId,
                DateTime.Now, new CatalogKey(viewModel.Shirt.Code), filter);

            Money lowestPrice = viewModel.FilteredPrices
                .OrderBy(p => p.UnitPrice).First().UnitPrice;

            Money lowestAllowed = viewModel.OptimizedPrices
                .Where(p => p.CustomerPricing.PriceTypeId == (CustomerPricing.PriceType)3)
                .First().UnitPrice;

            var promos = _promotionEngine.Evaluate(viewModel.Shirt.ContentLink);

            if (promos.Count() > 0)
            {
                viewModel.PromotionsTotal = promos.Sum(p => p.Percentage);
                lowestPrice -= lowestPrice * viewModel.PromotionsTotal / 100;
                if (lowestPrice < lowestAllowed)
                {
                    viewModel.SellingPrice = lowestAllowed;
                }
                else
                {
                    viewModel.SellingPrice = lowestPrice;
                    viewModel.PromotionsApplied = true;
                }
            }
            else viewModel.SellingPrice = lowestPrice;
        }
    }
}
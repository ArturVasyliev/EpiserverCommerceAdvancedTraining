using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Pricing;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce;
using CommerceTraining.Models.Catalog;
using Mediachase.Commerce.Catalog;
using EPiServer.Security;
//using CommerceTraining.SupportingClasses;C:\Episerver6\CommerceTraining\CommerceTraining\Controllers\StartPageController.cs
using Mediachase.Commerce.Security;
//using EPiServer.Security;
using Mediachase.Commerce.Orders.Managers;
using System;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Customers;
using System.Net.NetworkInformation;
using CommerceTraining.SupportingClasses;
using Mediachase.Commerce.Markets;
using CommerceTraining.Models.ViewModels;
using Owin;
using EPiServer.Filters;
using EPiServer.Commerce.Catalog.Linking;
using Mediachase.Commerce.InventoryService;
using System.Security.Principal;

namespace CommerceTraining.Controllers
{
    public class StartPageController : PageController<StartPage>
    {
        public readonly IContentLoader _contentLoader;
        public readonly UrlResolver _urlResolver;
        public readonly ICurrentMarket _currentMarketService;
        public readonly IMarketService _marketService;

        ContentReference topCategory { get; set; } // used for listing of nodes at the start-page

        public StartPageController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , ICurrentMarket currentMarket
            , IMarketService marketService)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _currentMarketService = currentMarket;
            _marketService = marketService;

            // uncomment the below when the catalog is modelled
            topCategory = contentLoader.Get<StartPage>(PageReference.StartPage).Settings.topCategory;
        }

        // A check for FInd
        private bool IsOnLine()
        {
            //return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            return false;
        }

        public string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

        public ActionResult Index(StartPage currentPage, string selectedMarket)
        {
            TempData["IsOnLine"] = CheckIfOnLine.IsInternetAvailable; // not the best way... improve this

            if (selectedMarket != null)
            {
                _currentMarketService.SetCurrentMarket(new MarketId(selectedMarket)); // outcommented in Adv.-starter
            }

            // just testing
            //Buy(currentPage);
            //CheckOnParentAndNode();
            //CheckInventory(); // native
            //CheckInventory2(); // AdjustOrRemove

            var model = new CommerceTraining.Models.ViewModels.PageViewModel<StartPage>(currentPage)
            {
                MainBodyStartPage = currentPage.MainBody,
                myPageChildren = _contentLoader.GetChildren<IContent>(currentPage.ContentLink),
                Customer = LoggedInOrNot(),

                // uncomment the below when the catalog is modelled
                topLevelCategories = FilterForVisitor.Filter(
                _contentLoader.GetChildren<CatalogContentBase>(topCategory).OfType<NodeContent>()),
                markets = _marketService.GetAllMarkets(),
                selectedMarket = _currentMarketService.GetCurrentMarket().MarketName,
                //selectedMarket = MarketId.Default.Value - starter in Adv.
                someInfo = GetStringInfo(currentPage)
            };

            return View(model);
        }

        Injected<IOrderGroupFactory> _factory;
        Injected<IOrderRepository> _repo;
        Injected<IInventoryProcessor> _processor;
        private void CheckInventory2()
        {
            string entryCode = "PriceTest_1";

            ICart cart = _repo.Service.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");
            ILineItem li = _factory.Service.CreateLineItem(entryCode, cart);
            li.Quantity = 1;
            cart.AddLineItem(li);
            cart.GetFirstShipment().WarehouseCode = "test";
            _orderRepository.Save(cart);

            var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

            cart.AdjustInventoryOrRemoveLineItems((item, issue) => validationIssues.Add(item, issue));

            /*
            _processor.Service.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                , OrderStatus.InProgress, (item, issue) => validationIssues.Add(item, issue));
            // is li removed? did'nt get "issue"
            */
            _orderRepository.Save(cart);

            
        }

        Injected<IInventoryService> invSrvs;
        private void CheckInventory()
        {
            string warehouseCode = "Test";
            string entryCode = "PriceTest_1";
            int quantity = 2;

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem(); // The one we use now
            requestItem.CatalogEntryCode = entryCode;
            requestItem.Quantity = quantity;
            requestItem.WarehouseCode = warehouseCode;
            requestItem.RequestType = InventoryRequestType.Purchase; // reserve for now
            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest =
                new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = invSrvs.Service.Request(inventoryRequest);
            bool theBool = false;

            if (inventoryResponse.IsSuccess)
            {
                theBool = inventoryResponse.IsSuccess;
            }
            else
            {

                InventoryResponseItem iii = inventoryResponse.Items.FirstOrDefault();
                InventoryResponseTypeInfo typeInfo = iii.ResponseTypeInfo;

            }

            /*
             
             
             */

        }

        private IEnumerable<string> GetStringInfo(StartPage currentPage)
        {
            List<string> localInfo = new List<string>();
            localInfo.Add("Shipping info");

            return localInfo;
        }

        //[HttpPost]
        //public ActionResult SetMarket(string MarketId) 
        //{
        //    //ServiceLocator.Current.GetInstance<ICurrentMarket>().SetCurrentMarket(new MarketId(MarketId));
        //    _currentMarketService.SetCurrentMarket(new MarketId(MarketId));

        //    return RedirectToAction("Index", new { node = ContentReference.StartPage });
        //}

        private string LoggedInOrNot()
        {
            if (CustomerContext.Current.CurrentContact != null)
            {
                return "Hi " + CustomerContext.Current.CurrentContact.FirstName;
            }
            else
            {
                return "Hi anonymous, join the club";
            }
        }

        // Check if on-line (for Find - in variation and checkout controllers)
        //public bool IsInternetAvailable
        //{
        //    get { return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() && _CanPingGoogle(); }
        //}

        //private static bool _CanPingGoogle()
        //{
        //    const int timeout = 1000;
        //    const string host = "google.com";

        //    var ping = new Ping();
        //    var buffer = new byte[32];
        //    var pingOptions = new PingOptions();

        //    try
        //    {
        //        var reply = ping.Send(host, timeout, buffer, pingOptions);
        //        return (reply != null && reply.Status == IPStatus.Success);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        #region NewOrderSystemSneakPeek

        //#region Checkout with a few new things added

        #region Services

        static IContentLoader _contentLoader2 = ServiceLocator.Current.GetInstance<IContentLoader>();
        static IOrderRepository _orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
        static IPriceService _priceService = ServiceLocator.Current.GetInstance<IPriceService>();
        static IPromotionEngine _promotionEngine = ServiceLocator.Current.GetInstance<IPromotionEngine>();
        static ICurrentMarket _currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
        static IShippingCalculator _shipmentCalculator = ServiceLocator.Current.GetInstance<IShippingCalculator>();
        static ReferenceConverter _refConv = ServiceLocator.Current.GetInstance<ReferenceConverter>();
        static IOrderGroupTotalsCalculator _totti = ServiceLocator.Current.GetInstance<IOrderGroupTotalsCalculator>();

        #endregion

        public ActionResult Index2(StartPage currentPage)
        {
            ContentReference nodeRef = _refConv.GetContentLink("");
            var catalog = _contentLoader2.Get<CatalogContent>(nodeRef);
            ViewBag.Variants = _contentLoader2.GetChildren<ShirtVariation>(nodeRef).ToList();

            var cart = LoadOrCreateCart();
            var totals = ((IOrderGroup)cart).GetTotal();

            ViewBag.Cart = cart;
            ViewBag.Totals = totals;
            ViewBag.LineItemsTotals = totals;//(cart.OrderForms.First())[cart.OrderForms.First().Shipments.First()];

            return View(currentPage);
        }

        private static void FillInAddress(OrderAddress shippingAddress)
        {
            shippingAddress.FirstName = "MyFirstName";
            shippingAddress.LastName = "MyLastName";
            shippingAddress.Email = "MyEmail";
            shippingAddress.CountryName = "MyCountry";
            shippingAddress.Line1 = "MyAddress";
            shippingAddress.PostalCode = "MyZipCode";
            shippingAddress.City = "MyCity";
        }

        private LineItem CreateLineItem(VariationContent variation, decimal quantity, decimal price)
        {
            LineItem lineItem = new LineItem();
            lineItem.DisplayName = variation.DisplayName;
            lineItem.Code = variation.Code;
            lineItem.MaxQuantity = variation.MaxQuantity.HasValue ? variation.MaxQuantity.Value : 100;
            lineItem.MinQuantity = variation.MinQuantity.HasValue ? variation.MinQuantity.Value : 1;
            lineItem.Quantity = quantity;
            lineItem.WarehouseCode = string.Empty; // may not know yet
            lineItem.InventoryStatus = variation.TrackInventory ? (int)InventoryStatus.Enabled : (int)InventoryStatus.Disabled;
            lineItem.ListPrice = price;
            lineItem.PlacedPrice = price;
            var productLink = variation.GetParentProducts().FirstOrDefault();
            if (productLink != null)
            {
                var product = _contentLoader.Get<ProductContent>(productLink);
                lineItem.ParentCatalogEntryId = product.Code;
            }
            return lineItem;
        }

        private static Cart LoadOrCreateCart()
        {
            // A LoadOrCreate function has been added in 9.2 (still a cover-up of the old stuff in 9.1)
            // This code will create a cached cart, but it going to hit the database on the first request
            return _orderRepository.Load<Cart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName).FirstOrDefault()
                ?? _orderRepository.Create<Cart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName);
        }

        [HttpPost]
        public ActionResult Buy(StartPage currentPage)
        {
            var cart = LoadOrCreateCart();
            var market = _currentMarket.GetCurrentMarket();
            // Add shipping
            var shippingMethod = ShippingManager.GetShippingMethodsByMarket
                (market.MarketId.Value, false).ShippingMethod.FirstOrDefault();

            cart.OrderAddresses.Clear();
            cart.OrderForms.First().Shipments.Clear();
            cart.OrderForms.First().Payments.Clear();

            var shippingAddress = cart.OrderAddresses.AddNew();
            FillInAddress(shippingAddress);

            var shipment = new Shipment();
            var shipmentId = cart.OrderForms.First().Shipments.Add(shipment);
            shipment.ShippingMethodId = shippingMethod.ShippingMethodId;
            shipment.ShippingMethodName = shippingMethod.Name;
            shipment.SubTotal = shippingMethod.BasePrice;

            // LineItem

            ContentReference theRef = _refConv.GetContentLink("Long-Sleeve-Shirt-White-Small_1");
            VariationContent theContent = _contentLoader.Get<VariationContent>(theRef);
            LineItem li = CreateLineItem(theContent, 2, 22);

            var orderForm = cart.OrderForms.First();
            orderForm.LineItems.Add(li);
            var index = orderForm.LineItems.IndexOf(li);
            cart.OrderForms.First().Shipments.First().AddLineItemIndex(index, li.Quantity);


            //var liId = cart.OrderForms.First().LineItems.Add(li);
            //PurchaseOrderManager.AddLineItemToShipment(cart, 1, shipment, 2);

            // Add a pay method
            var paymentMethod = PaymentManager.GetPaymentMethodsByMarket(market.MarketId.Value)
                .PaymentMethod.First();
            // Add Payment
            var payment = cart.OrderForms.First().Payments.AddNew(typeof(OtherPayment));
            payment.Amount = 42; // ((IOrderGroup)cart).GetTotal(_totti).Amount;// .SubTotal.Amount;
            payment.PaymentMethodName = paymentMethod.Name;
            payment.PaymentMethodId = paymentMethod.PaymentMethodId;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionID = "transactionId";

            // No activations of Ship&Pay&Tax-providers in this example 

            // Do the purchase
            using (var scope = new Mediachase.Data.Provider.TransactionScope())
            {
                OrderReference oRef = _orderRepository.SaveAsPurchaseOrder(cart);
                // I want to do this _orderRepository.Delete(cart);
                //_orderRepository.Delete(((IOrderGroup)cart).OrderLink);
                scope.Complete();
            }

            return Content("Done");
        }

        #endregion // checkout

        #region CalculationServices Injected

        Injected<IOrderRepository> _oRep;
        Injected<IContentLoader> _loader;

        Injected<IShippingCalculator> _shipCalc; // shipping totals
        Injected<ILineItemCalculator> _lItemCalc; // extended price ... moving away from Ext.Pr.
        Injected<IOrderGroupCalculator> _ogCalc; // totals
        Injected<ITaxCalculator> _taxCalc; // tax totals
        Injected<IOrderFormCalculator> _ofCalc; // totals
        Injected<ICurrentMarket> _currMarket;

        #endregion

        public void CheckOnCalc() // (Cart theCart, Guid id)
        {
            Cart theCart = LoadOrCreateCart();
            //CartHelper ch = new CartHelper(theCart); // lazy now
            if (theCart.OrderForms[0].LineItems.Count() > 0) // before WF-exec
            {
                // Do calc
            }
            else // just looking
            {
                // add a LineItem
                //ILineItem lineItem = new // nope
                //_oRep.Service. // nope nothing about LineItems
                // seems to need "OldSchool" ... doing it lazy for now
                //ch.AddEntry(CatalogContext.Current.GetCatalogEntry("Some-Sox_1")); // have a look at "LoadOrCreateCart"

                ContentReference theRef = _refConv.GetContentLink("Long-Sleeve-Shirt-White-Small_1");
                VariationContent theContent = _contentLoader.Get<VariationContent>(theRef);
                LineItem li = CreateLineItem(theContent, 2, 22);

                var orderForm = theCart.OrderForms.First();
                orderForm.LineItems.Add(li);
                var index = orderForm.LineItems.IndexOf(li);
                theCart.OrderForms.First().Shipments.First().AddLineItemIndex(index, li.Quantity);
            }

            // just checking
            WorkflowResults wfResult = OrderGroupWorkflowManager.RunWorkflow
                    (theCart, OrderGroupWorkflowManager.CartValidateWorkflowName);

            IMarket market = _currentMarket.GetCurrentMarket();
            Currency curr = theCart.BillingCurrency; // og.Currency;

            Guid id = new Guid("097361ec-a4ac-4671-9f2a-a56e3b6f7e97");
            IOrderGroup og = _oRep.Service.Load(id, theCart.Name).FirstOrDefault();
            IOrderForm form = og.Forms.FirstOrDefault();
            IShipment ship = form.Shipments.FirstOrDefault(); // there is a shipment there (...is a "bigger change")

            //CartHelper ch = new CartHelper((Cart)og);
            int liId = form.Shipments.FirstOrDefault().LineItems.FirstOrDefault().LineItemId; // okay

            Shipment otherShip = theCart.OrderForms[0].Shipments.FirstOrDefault(); // no ship here...?
            // it's not added yet the old-school way
            int shipments = theCart.OrderForms[0].Shipments.Count; // zero...?

            //otherShip = (Shipment)ship;
            //int ShipId = theCart.OrderForms[0].Shipments.Add(otherShip); // Gets ordinal index it seems ... not ShipmentId
            // okay, but...



            ILineItem Ili = form.Shipments.FirstOrDefault().LineItems.FirstOrDefault();

            var dtoShip = ShippingManager.GetShippingMethodsByMarket
                (_currMarket.Service.GetCurrentMarket().MarketId.Value, false).ShippingMethod.FirstOrDefault();
            Shipment s = new Shipment();
            s.ShippingMethodId = dtoShip.ShippingMethodId;
            s.ShippingMethodName = dtoShip.Name;
            int ShipId = theCart.OrderForms[0].Shipments.Add(s);

            // ..seems to work, 
            //PurchaseOrderManager.AddLineItemToShipment(
            //  theCart, Ili.LineItemId, s, 2);
            // probably need to persist (old way) & reload "the new way"
            //ILineItem li2 = form.Shipments.FirstOrDefault().LineItems.FirstOrDefault(); // new way (null)

            // OrderForm
            Money formTot = _ofCalc.Service.GetTotal(form, market, curr);

            // OrderGroup
            Money handlingFee = _ogCalc.Service.GetHandlingTotal(theCart);
            Money subTotal = _ogCalc.Service.GetSubTotal(theCart);
            Money total = _ogCalc.Service.GetTotal(theCart);

            // Shipping
            //var shipCost = _shipCalc.Service.GetShipmentCost(form, market, curr);
            var shipTot = _shipCalc.Service.GetShippingItemsTotal(ship, curr);

            //LineItems
            var x = _lItemCalc.Service.GetExtendedPrice(theCart.OrderForms.FirstOrDefault().LineItems.FirstOrDefault(), curr); // Ext.Price verkar vara på väg ut 

            //Taxes
            var t = _taxCalc.Service.GetTaxTotal(form, market, curr);

        }

        //#endregion // new stuff

        public void CheckOnParentAndNode()
        {
            List<ContentReference> theList = new List<ContentReference>();
            var shirt = _refConv.GetContentLink("Long Sleeve Shirt White Small_1");
            var noProdParent = _refConv.GetContentLink("PriceTest_1");
            var Pack = _refConv.GetContentLink("SomePackage_1");
            var Prod = _refConv.GetContentLink("Shirt-Long-Sleeve_1");
            var node = _refConv.GetContentLink("Shirts_1");

            theList.Add(shirt); // parent = node, ...typeId "Variation" ... no children
            theList.Add(noProdParent);// parent = node ...typeId "Variation" ... no children
            theList.Add(Pack);// parent = node ...typeId "Package" ... no children
            theList.Add(Prod); //parent = node...typeId ""...no children
            theList.Add(node);

            // check ...TypeId - string/int

            var rel = new NodeEntryRelation
            {
                // IsPrimary
                // TargetCatalog
            };

            var rel2 = new PackageEntry
            {
                //GroupName
                //Quantity
                //SortOrder
            };

            var stuff = _contentLoader.GetItems(theList, new LoaderOptions());

            foreach (var item in stuff)
            {
                var Parent = _contentLoader.Get<CatalogContentBase>(item.ParentLink);

                // new...
                var children = _contentLoader.GetChildren<CatalogContentBase>(item.ContentLink);

                var ii = item.GetOriginalType().Name; // "ShirtVariation" ... have FullName also

                if (item is EntryContentBase) // use this, checks "the tree"
                {
                    var ParentPackages = // Smashing on the node... of course - "Wrong base class"
                        _contentLoader.Get<EntryContentBase>(item.ContentLink).GetParentPackages();

                    var ParentProducts =
                        _contentLoader.Get<EntryContentBase>(item.ContentLink).GetParentProducts();

                    var ParentEntries =
                        _contentLoader.Get<EntryContentBase>(item.ContentLink).GetParentEntries();

                    var ParentCategories =
                        _contentLoader.Get<EntryContentBase>(item.ContentLink).GetCategories();
                }
            }

            // Can do like this now, not tested yet
            //var children2 = _relationRepository.GetChildren<NodeEntryRelation>(parentLink);


        }

    }
}
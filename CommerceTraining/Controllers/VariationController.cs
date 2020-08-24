using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using CommerceTraining.Models.Pages;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Orders;
using EPiServer.Commerce.Catalog;
using CommerceTraining.Models.ViewModels;
using CommerceTraining.SupportingClasses;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Catalog;
using System;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce;
using EPiServer.Security;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Globalization;
using System.Globalization;
using System.Web.Routing;
using System.Web;
using Mediachase.Commerce.Catalog.Managers;
using EPiServer.Commerce.SpecializedProperties;

using Mediachase.Commerce.Pricing;
using CommerceTraining.Infrastructure.Pricing;
using System.Text;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders.Dto;
using CommerceTraining.Infrastructure.CartAndCheckout;
using Mediachase.Commerce.Catalog.Dto;
using EPiServer.Commerce.Marketing;
using CommerceTraining.Infrastructure;
using System.Collections;
using EPiServer.Find;
using EPiServer.Web;
using EPiServer.Framework.Web;
using Mediachase.Commerce.Plugins.Shipping;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Security;
using EPiServer.Commerce.Order.Internal;
using Mediachase.MetaDataPlus;

namespace CommerceTraining.Controllers
{
    public class VariationController : CatalogControllerBase<ShirtVariation>
    {
        // ToDo: PricingService (custom) is called/created multiple times - fix that  
        // Have CheckPrices For Extensions & LoadingExamples

        //private IEnumerable<ValidationIssue> ValidationIssuesClassLevel;
        private Dictionary<ILineItem, ValidationIssue> ValidationIssuesClassLevel = new Dictionary<ILineItem, ValidationIssue>();

        private readonly IPriceService _priceService;
        private readonly IPriceDetailService _priceDetailService;
        //private readonly ICurrentMarket _currentMarket2;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPromotionEngine _promotionEngine;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        public readonly ICurrentMarket _currentMarketService;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IInventoryService _inventoryService;
        private readonly MyPriceCalculator _myPriceCalculator;

        static Injected<IContentLoader> xyz;

        private static bool IsOnLine { get; set; } // QuickFix

        // added for Adv. Cart clean, check/add second ship & pay
        CartAndCheckoutService ccService = new CartAndCheckoutService();
        CustomTaxManager customTaxManager = new CustomTaxManager();


        // ToDo: move to .ctor
        //Injected<ICatalogSystem> _cat; // just to demo
        //Injected<ReferenceConverter> _refConv;
        //Injected<PricingLoader> _priceLoader;

        public VariationController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver // use this in node listing instead
            , IPriceService priceService
            , IPriceDetailService pricedetailService
            , ICurrentMarket currentMarket
            , IPromotionEngine promotionEngine
            , IOrderRepository orderRepository
            , IOrderGroupFactory orderGroupFactory
            , ILineItemCalculator lineItemCalculator
            , ILineItemValidator lineItemValidator
            , IPlacedPriceProcessor placedPriceProcessor
            , ICurrentMarket currentMarketService
            , IInventoryService inventoryService
            , IWarehouseRepository warehouseRepository
            , MyPriceCalculator myPriceCalculator
        )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver,currentMarket)
        {
            _priceService = priceService;
            _priceDetailService = pricedetailService;
            //_currentMarket2 = currentMarket;
            _promotionEngine = promotionEngine;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _lineItemCalculator = lineItemCalculator;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _currentMarketService = currentMarketService;
            _inventoryService = inventoryService;
            _warehouseRepository = warehouseRepository;
            _myPriceCalculator = myPriceCalculator;
        }

        public ActionResult DoLowLevelDtoEdit(ShirtVariation currentContent, string newText)
        {
            //Old school way to change using Dtos directly
            var catSystem = ServiceLocator.Current.GetInstance<ICatalogSystem>();
            var shirtDto = catSystem.GetCatalogEntryDto(currentContent.Code);
            var entryRow = shirtDto.CatalogEntry.First();
            MetaObject metaObject = MetaObject.Load(MetaDataContext.Instance, entryRow.CatalogEntryId, entryRow.MetaClassId);
            metaObject["MainBody"] = newText;
            metaObject.AcceptChanges(MetaDataContext.Instance);

            return RedirectToAction("Index");
        }


        public ActionResult Index(ShirtVariation currentContent)
        {
            IsOnLine = CheckIfOnLine.IsInternetAvailable; // Need to know... for Find
            CheckWarehouses(currentContent); // WH-info on the Page

            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            var cartUrl = _urlResolver.GetUrl(startPage.Settings.cartPage, currentContent.Language.Name);
            var wUrl = _urlResolver.GetUrl(startPage.Settings.cartPage, currentContent.Language.Name);

            PricingService pSrvs = new PricingService(
                _priceService, _currentMarket, _priceDetailService);
            // used elsewere
            //currentContent.GetCustomerPrices() // works with the R/O-pricingLoader, would miss the custom SaleTypes
            //currentContent.GetPrices() // this one also uses the R/O-Loader ... unusable

            #region Newpromotions

            // New promotions
            decimal savedMoney = 0;
            string rewardDescription = String.Empty;
            //IEnumerable<RewardDescription> rewards;

            // get prices incl. "BasePrice"
            //IPriceValue salePrice = BestPricingCalculatorEver.GetSalePrice(currentContent.ContentLink);//
            IPriceValue salePrice = _myPriceCalculator.GetSalePrice(currentContent, 1);
            // the below does the second "Evaluate"
            var descr = _promotionEngine.Evaluate(currentContent.ContentLink).ToList();
            if (descr.Count == 0) // No promos
            {
                var d = new RewardDescription(
                    FulfillmentStatus.NotFulfilled, null, null, 0, 0, RewardType.None, "No promo");
                descr.Add(d);
                rewardDescription = descr.First().Description; // ...just to show
            }
            else
            {
                foreach (var item in descr)
                {
                    rewardDescription += item.Description;
                }
            }

            // previous way
            if (descr.Count() >= 1)
            {
                savedMoney = descr.First().Percentage * salePrice.UnitPrice.Amount / 100;
                //rewardDescription = descr.First().Description;
                Session["SavedMoney"] = savedMoney;
            }
            else
            {
                savedMoney = 0;
                //rewardDescription = "No discount";
            }

            // ...this goes to PriceCalc-discount
            var promoPrice = salePrice.UnitPrice.Amount - savedMoney;

            #endregion

            #region just checking on promos
            //List<RewardDescription> rewards = new List<RewardDescription>();
            //rewards = _promotionEngine.Evaluate(currentContent.ContentLink).ToList();
            //IEnumerable<DiscountedEntry> entries = _promotionEngine.GetDiscountPrices(currentContent.ContentLink, _currentMarket.GetCurrentMarket());
            #endregion

            #region FindStuff


            // new Find-Stuff - need to check this out... 
            // FindQueries Qs = new FindQueries();
            // Qs.NewExtensionMethods(currentContent);
            // Qs.SDKExamples(currentContent.ContentLink);
            #endregion

            #region Checking some for routing and http-ctx
            /*
            var str = currentContent.GetOriginalType().Name; // not for redirect
            var x = ServiceLocator.Current.GetInstance<TemplateResolver>();
           
            RequestContext requestContext = new RequestContext(base.HttpContext, base.RouteData);
            var parentStack = requestContext.HttpContext.Items[ContentContext.ContentContextKey] as
               Stack<ContentContext.ContentPropertiesStack>;

            string controller = requestContext.GetController();
            var t = x.Resolve(requestContext.HttpContext, new CartPage(), TemplateTypeCategories.MvcController); // could use .Name
            */
            #endregion

            #region LookingAround (incl. new and old Promo-engine)

            //LoadingExamples(currentContent);
            //CheckPrices(currentContent);

            // Have to fake a cart... but it looks good
            //string rewardDescription = String.Empty;

            //decimal savedMoney = CheckBetaPromotions(currentContent, out rewardDescription);

            //var theProxy = currentContent.GetType();
            //var yourClass = currentContent.GetOriginalType();

            //StoreHelper.GetDiscountPrice(currentContent.LoadEntry());
            //StoreHelper.GetSalePrice(currentContent.LoadEntry()

            #endregion

            #region Relations, parent - child, etc.

            CheckOnRelations(currentContent);

            #endregion

            #region RoCe - check this

            // quick-fix - nothing back
            ICart dummyCart = _orderRepository.LoadOrCreateCart<ICart>(new Guid(), "DummyCart");
            ILineItem lineItem = _orderGroupFactory.CreateLineItem(currentContent.Code, dummyCart);
            var c2 = _lineItemCalculator.GetExtendedPrice(lineItem, _currentMarket.GetCurrentMarket().DefaultCurrency);
            var check = _lineItemCalculator.GetDiscountedPrice(
                lineItem, _currentMarket.GetCurrentMarket().DefaultCurrency).Amount;

            // Should check the BasePrice here (new way)
            // BestPricingCalc is the old way
            // should override the LI-calculator

            #endregion

            //_currentMarket.GetCurrentMarket().DefaultCurrency.Format(cu)
            //Currency.SetFormat(_currentMarket.GetCurrentMarket().DefaultCurrency.Format.CurrencySymbol);
            string thePriceString = string.Empty;

            if (currentContent.GetDefaultPrice().UnitPrice.Amount == 0)
            {
                thePriceString = "no default price";
            }
            else
            {
                thePriceString = currentContent.GetDefaultPrice().UnitPrice.ToString();
            }

            var model = new ShirtVariationViewModel
            {
                MainBody = currentContent.MainBody,

                // Pricing
                priceString = thePriceString,
                //theRightPriceToPlace = GetThePriceToPlace(currentContent), // tiered pricing...old, not in use
                CustomerPricingPrice = GetCustomerPricingPrice(currentContent),
                //discountPrice = CustomStoreHelper.GetDiscountPrice(currentContent.LoadEntry(CatalogEntryResponseGroup.ResponseGroup.Nodes)),
                discountPriceNew = _lineItemCalculator.GetDiscountedPrice(lineItem, _currentMarket.GetCurrentMarket().DefaultCurrency).Amount,

                image = GetDefaultAsset((IAssetContainer)currentContent),
                CanBeMonogrammed = currentContent.CanBeMonogrammed,
                ProductArea = currentContent.ProductArea,
                CartUrl = cartUrl, // new stuff - not yet in course
                WishlistUrl = wUrl, // new stuff - not yet in course

                // Added for Adv. below
                //labPrice = BestPricingCalculatorEver.GetDiscountPrice(currentContent, 1, promoPrice), // , "Fashion", "Shirts"
                labPrice = _myPriceCalculator.CheckDiscountPrice(currentContent, 1, promoPrice),
                overridePrices = pSrvs.GetPrices(currentContent.Code),
                PromoString = rewardDescription, // in #region LookingAround
                betaDiscountPrice = savedMoney, // in #region LookingAround

                // warehouse info (Lists get filled up in the entrance of "Index-method")
                generalWarehouseInfo = this.generalWarehouseInfo,
                specificWarehouseInfo = this.specificWarehouseInfo, // Fills up "Specific-List"
                localMarketWarehouses = GetLocalMarketWarehouses(),
                entryCode = currentContent.Code,

                //Markets
                currentMarket = GetCurrentMarket(),
                marketOwner = GetMarketOwner(),

                // Associations
                //Associations = GetAssociatedEntries(currentContent), // IEnumerable<ContentReference> // Remove when Aggregation is done
                //AssociationMetaData = GetAssociationMetaData(currentContent), // string // Remove when Aggregation is done
                AssocAggregated = GetAggregatedAssocciations(currentContent), // Dictionary<string, ContentReference> // The final thing

                // Searchendizing ... need to be OnLine to use
                BoughtThisBoughtThat = GetOtherEntries(currentContent.Code),

                // Taxes
                Tax = GetTaxOldSchool(currentContent),
                TaxString = GetTaxStrings(currentContent),
                TaxNewSchool = GetTaxNewSchool(currentContent),

                // info about the variation
                VariationAvailability = currentContent.IsAvailableInCurrentMarket(),
                VariationInfo = "Info: " + CollectInfo()
            };

            return View(model);
        }

        private void CheckOnRelations(ShirtVariation currentContent)
        {
            // it'a allways the node (the "IsPrimary" one)
            var p = currentContent.ParentLink;
            var c = _contentLoader.Get<CatalogContentBase>(p);

            // probably a better way
            var nodes = currentContent.GetCategories();
            var prods = currentContent.GetParentProducts();

        }

        private string CollectInfo()
        {
            // For now...
            return String.Empty;
        }

        public IEnumerable<string> GetTaxStrings(ShirtVariation currentContent)
        {
            List<string> tempTaxStrings = new List<string>();

            TaxValue[] taxes = customTaxManager.GetTaxes((int)currentContent.TaxCategoryId);

            if (taxes.Count() > 0)
            {
                foreach (TaxValue item in taxes)
                {
                    tempTaxStrings.Add(item.TaxType + " " + item.Name + " " + item.Percentage.ToString() + "%");
                }
            }
            else
            {
                tempTaxStrings.Add("...no taxes");
            }

            return tempTaxStrings;
        }

        private decimal GetTaxOldSchool(ShirtVariation currentContent)
        {
            TaxValue[] taxValues = customTaxManager.GetTaxes((int)currentContent.TaxCategoryId);
            if (taxValues.Count() > 0)
            {
                return (decimal)taxValues.FirstOrDefault().Percentage * GetThePriceToPlace(currentContent) / 100;
            }
            else
            {
                return 0;
            }

        }

        Injected<ITaxCalculator> _taxCalc;

        private string GetTaxNewSchool(ShirtVariation currentContent)
        {
            IMarket market = _currentMarket.GetCurrentMarket();
            Guid currCust = CustomerContext.Current.CurrentContactId;
            string bogusCart = "BogusCart";

            ICart cart = _orderRepository.LoadOrCreateCart<ICart>(
                currCust, bogusCart);

            ILineItem lineItem = _orderGroupFactory.CreateLineItem(currentContent.Code, cart);
            lineItem.Quantity = 1;
            lineItem.PlacedPrice = GetCustomerPricingPrice(currentContent).UnitPrice.Amount;
            lineItem.TaxCategoryId = currentContent.TaxCategoryId;
            cart.AddLineItem(lineItem);

            IOrderAddress bogusAddress = _orderGroupFactory.CreateOrderAddress(cart);
            bogusAddress.CountryCode = "sv";
            bogusAddress.City = "Stockholm";
            bogusAddress.CountryName = "Sweden";

            string str = String.Empty;
            //str += _taxCalc.Service.GetTaxTotal(cart, market, market.DefaultCurrency).Amount.ToString();

            var taxValues = Enumerable.Empty<ITaxValue>();
            taxValues = OrderContext.Current.GetTaxes(Guid.Empty, currentContent.theTaxCategory, "sv", bogusAddress);

            str = _taxCalc.Service.GetSalesTax(lineItem, market, bogusAddress, new Money(0m,"SEK")).ToString();
            

            return str;
        }

        private decimal GetThePriceToPlace(ShirtVariation currentContent)
        {
            PricingService myPricing = new PricingService(
             _priceService, _currentMarket, _priceDetailService);

            return myPricing.GetTheRightPrice(currentContent); // for the group and so forth
        }

        private EPiServer.Commerce.SpecializedProperties.Price GetCustomerPricingPrice(ShirtVariation currentContent)
        {
            // This is the original...
            PricingService myPricing = new PricingService(
             _priceService, _currentMarket, _priceDetailService);

            return myPricing.GetTheRightCustomerPrice(currentContent); // for the group and so forth

        }

        private string GetCurrentMarket()
        {
            return _currentMarket.GetCurrentMarket().MarketName;
        }

        // Remove when cleaning up - just looking around
        private void CheckNewPromotions(ShirtVariation currentContent)
        {
            //ServiceLocator.Current.GetInstance<IPromotionEngine>().Run(OrderGroup);
            PromotionProcessorResolver pr = ServiceLocator.Current.GetInstance<PromotionProcessorResolver>();
            IContentLoader cl = ServiceLocator.Current.GetInstance<IContentLoader>();
            CampaignInfoExtractor ci = ServiceLocator.Current.GetInstance<CampaignInfoExtractor>();

        }

        public ActionResult AddToCart
            (ShirtVariation currentContent, decimal Quantity, string Monogram) // 
        {
            // New
            var cart = _orderRepository.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");
            //OrderContext.Current.GetCart()

            // new line, check in TrousersController
            cart.Properties["SpecialShip"] = false;

            ICollection<IPayment> payments = cart.GetFirstForm().Payments;

            string code = currentContent.Code;

            var lineItem = cart.GetAllLineItems().SingleOrDefault(x => x.Code == code);
            if (lineItem == null)
            {
                lineItem = _orderGroupFactory.CreateLineItem(code, cart);
                lineItem.Quantity = Quantity;

                _placedPriceProcessor.UpdatePlacedPrice
                    (lineItem, GetContact(), _currentMarket.GetCurrentMarket().MarketId, cart.Currency
                    , (lineItemToValidate, ValidationIssue) => { });

                // older
                var justChecking = lineItem.GetDiscountedPrice(cart.Currency);
                
                // new, just a check
                var dd = _promotionEngine.Evaluate(currentContent.ContentLink);
                if (dd.Count() != 0)
                {
                    var ddd = dd.First().SavedAmount;
                }
                else
                {
                    var ddd = 0;
                }
                
                //lineItem.TrySetDiscountValue(()
                //ILineItemDiscountAmount lineItemDiscountAmount = null;
                //lineItemDiscountAmount.EntryAmount = 10;

                cart.AddLineItem(lineItem);

                // new - need the RewardDescriptions as well
                // don't get the discount for the first LI...?
                cart.ApplyDiscounts();
            }
            else
            {
                lineItem.Quantity += Quantity;
            }

            ccService.CheckOnLineItem(currentContent, lineItem);

            // new 
            var d = lineItem.GetEntryDiscount();

            #region Could be more pro-active - checking Market & Inventory before adding the LI

            var validationIssues = new Dictionary<ILineItem, ValidationIssue>();
            var validLineItem = _lineItemValidator.Validate(lineItem, _currentMarket.GetCurrentMarket().MarketId
                , (item, issue) => ValidationIssuesClassLevel.Add(item, issue));

            // crash here if the previous lines fond issues... like "not in market"
            cart.ValidateOrRemoveLineItems((item, issue) => validationIssues.Add(item, issue));

            #endregion

            if (validLineItem)
            {
                // not good, but...as the SDK says
                /* -- Line item calculator -- 
                 * The LineItemCalculator calculates a line item's extended price 
                 *   and discounted price.Extended price. Includes order-level discount amount 
                 *   (spread over all line items in the shipment) and any line item discount amount.
                 * Discounted price. Only calculates the price with the "line item discount amount," 
                 * that is, the discount for a specific item.*/

                // ...have to do something like this for now - out-commented for now
                //LineItem li = (LineItem)lineItem;
                //li.LineItemDiscountAmount = (decimal)Session["SavedMoney"]; // ...and it gets into CM

                // doesn't seem "to bite"
                //lineItem.Properties["LineItemDiscountAmount"] = (decimal)Session["SavedMoney"];
                //InMemoryLineItem memLI = (InMemoryLineItem)lineItem;

                lineItem.Properties["Monogram"] = Monogram;
                lineItem.Properties["RequireSpecialShipping"] = currentContent.RequireSpecialShipping;

                // Below - need to set the cart.Properties["SpecialShip"] = false; ... it's null now
                // Bug in 11, rewritten for now
                //ccService.IsSecondShipmentReqired(cart);
                ccService.CheckOnLineItems(cart);

                // new October 2017 - gives the discount set properly in the cart
                IEnumerable<RewardDescription> discounts = cart.ApplyDiscounts();

                _orderRepository.Save(cart);

                // dummy for now... RoCe: Fix
                Session["SecondPayment"] = false;
            }

            string passingValue = cart.Name; // if needed, send something along

            ContentReference cartRef = _contentLoader.Get<StartPage>(ContentReference.StartPage).Settings.cartPage;

            // get to the cart page, if needed
            return RedirectToAction("Index", new { node = cartRef, routedData = passingValue });

            #region Checking on Redirects/Transfers
            // workaround for RedirectToAction (as we do from Cart --> CheckOut)... doesn´t work at the moment from ECF-pages 
            //Url url = _urlResolver.GetUrl(cartRef);
            //CartPage cartPage = _contentLoader.Get<CartPage>(cartRef);
            //var lang2 = ContentLanguage.PreferredCulture;
            //var name = cartPage.Name;

            //return RedirectToAction("Index", lang2 + "/" + name, new { passedAlong = passingValue }); // Change this in "Fund"
            //return RedirectToAction("Index", "en/Cart", new { passedAlong = passingValue }); // works
            //string passingValue = ch.Cart.Name; // if needed, the cart for example
            //return RedirectToAction("Index", new { node = theRef, passedAlong = passingValue }); // nothing is happening

            //return (ActionResult)View(model); //nope

            //return RedirectToAction("Index", new { node = theRef, passedAlong = passingValue }); // njet
            //Server.Transfer("http://localhost:49918" + _urlResolver.GetUrl(theRef.ToPageReference())); // njet

            //return RedirectToAction("Index", new { node = theRef.ToPageReference(), passedAlong = passingValue }); 
            //return RedirectToRoute()

            //              return  RedirectToAction( new RouteValueDictionary()
            //     new{ 
            //          "controller" = Blah, 
            //          "action" = Blah, 
            //          "foo" = “Bar” 
            //     } 
            //));

            // var x = HttpContext.Server.TransferRequest("");
            // Response.Redirect(_urlResolver.GetUrl(theRef)); // OK

            //return RedirectToAction("Index",url.Uri.OriginalString , new { passedAlong = passingValue });  // nope

            //Redirect(url.ToString());
            //return null;

            #endregion

        }

        // Optional lab in Fund. Mod. D - create a WishList and inspection in CM + config -alteration
        public void AddToWishList(ShirtVariation currentContent)
        {
            CartHelper wishList = new CartHelper(CartHelper.WishListName); // note the arg.
            wishList.AddEntry(currentContent.LoadEntry());
            wishList.Cart.AcceptChanges();

            string passingValue = currentContent.Code;
            ContentReference theRef = currentContent.ParentLink; // maybe go here when done

            // just testing
            Response.Redirect(_urlResolver.GetUrl(theRef));
            //RedirectToRoute()

            #region Chwcking links

            //var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            //var cartUrl = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(startPage.Settings.cartPage, currentContent.Language.Name);

            ////return RedirectToAction("Index","CartController", new { node = theRef, passedAlong = passingValue });

            //var lang = ContentLanguage.Instance.FinalFallbackCulture.ToString();  // gets "en"

            //return RedirectToAction("Index", "en/Cart", new { passedAlong = passingValue }); // works
            //return RedirectToAction("Index", CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "/Cart"
            //    , new { passedAlong = passingValue }); // works
            //// would you do like the above?

            //// Cut´n paste from Cart
            //StartPage home = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            //ContentReference theRef = home.Settings.checkoutPage;
            //string passingValue = "This is fun"; // could pass the cart instead

            // kolla Url.Action
            //Server.Transfer / Server.TransferRequest
            //Httputility
            #endregion

        }

        // Fund: Pricing Extensions // more of pricing in /Pricing/PricingService...
        private void CheckPrices(VariationContent currentContent)
        {
            /**/
            //currentContent.getp
            Entry e = CatalogContext.Current.GetCatalogEntry(currentContent.Code);
            var p = e.PriceValues; // gets 5

            IEnumerable<CatalogEntryDto.SalePriceRow> rows = CatalogContext.Current.GetCatalogEntryDto
                (currentContent.Code, new CatalogEntryResponseGroup
                    (CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull)).SalePrice; // zero rows
            /**/

            var salePrice = StoreHelper.GetSalePrice(currentContent.LoadEntry(), 7);
            var discountPrice = StoreHelper.GetDiscountPrice(currentContent.LoadEntry());

            var priceRef = currentContent.PriceReference; // a ContentReference

            EPiServer.Commerce.SpecializedProperties.ItemCollection<EPiServer.Commerce.SpecializedProperties.Price>
                gotPrices = currentContent.GetPrices(); // Gets all, recieve "Specialized" Price

            var defaultPrice = currentContent.GetDefaultPrice(); // All Cust + qty 0 ... market sensitive

            var custPrices = currentContent.GetCustomerPrices(
                new Currency("USD")
                , 3M
                , true); // for the current user - fourth arg is the PriceLoader - remember EffectivePriceGroup

            // the below includes TaxCategory
            var PriceStuff = (IPricing)currentContent; // null if "a price" is N/A,

            // not extensions, fairly full view of prices
            PricingService myPricing = new PricingService(
                _priceService, _currentMarket, _priceDetailService);

            //myPricing.CheckPrices(currentContent);
            //myPricing.GetPrices(currentContent.Code);

        }

        // lazy dev.
        private void LoadingExamples(ShirtVariation currentContent)
        {
            // some of this is / will be in Find
            //currentContent.LoadEntry(CatalogEntryResponseGroup.ResponseGroup.)

            ContentReference parent = currentContent.ParentLink; //...from "me" as the variation
            IEnumerable<EntryContentBase> nodeChildren = base._contentLoader.GetChildren<EntryContentBase>(parent);

            IEnumerable<ContentReference> allLinks = currentContent.GetCategories();

            IEnumerable<Relation> nodes = currentContent.GetNodeRelations();

            var theType = currentContent.GetOriginalType(); // handy

            var proxy = currentContent.GetType();

            IEnumerable<ContentReference> prodParents = currentContent.GetParentProducts();

            IEnumerable<ContentReference> parentPackages = currentContent.GetParentPackages();

            IMarket market = _currentMarketService.GetCurrentMarket();
            bool available = currentContent.IsAvailableInMarket(market.MarketId); // if we want to know about another market

            bool available2 = currentContent.IsAvailableInCurrentMarket();

            ISecurityDescriptor sec = currentContent.GetSecurityDescriptor();

            // CatalogContext.Current.GetCatalogEntriesDto()

            //currentContent.LoadEntryDt

            //ServiceLocator.Current.GetInstance<IOrderRepository>().Load<IOrderGroup>();

        }

        /* For Adv. below */
        #region Warehouse and Inventory

        List<string> generalWarehouseInfo = new List<string>();
        List<string> specificWarehouseInfo = new List<string>();

        // check what we have in stock
        private IEnumerable<string> GetLocalMarketWarehouses()
        {
            // can do in many ways...
            // ...using the Market code and CountryCode for the WH-Contact... could have a better plan :)
            var marketCode = _currentMarket.GetCurrentMarket().MarketId.Value;
            List<IWarehouse> warehouses = _warehouseRepository
                .List()
                .Where(w => w.ContactInformation.CountryCode == marketCode).ToList();

            foreach (var item in warehouses)
            {
                yield return item.Name;
            }
        }

        private void CheckWarehouses(ShirtVariation currentContent)
        {
            generalWarehouseInfo.Add("Entry inventory Tracked: " + currentContent.TrackInventory.ToString());

            // other code in older CommerceDemo\...\AdminPageTemplate
            // currentContent.ApplicationId == w.ApplicationId.ToString()) &&
            // how many FFCenters
            IEnumerable<IWarehouse> fullfillmentCenters = _warehouseRepository.List()
                .Where(w => ( w.IsActive && w.IsFulfillmentCenter));

            if (fullfillmentCenters.Count() > 1)
            {
                generalWarehouseInfo.Add("More than one fullfillment center... needs custom logic");
            }

            // get all WHs for enumeration and output to the view
            IEnumerable<IWarehouse> allWarehouses = _warehouseRepository.List();
            Decimal requestedQuantity = 0M;

            // ...there is an InventoryLoader
            foreach (var warehouse in allWarehouses)
            {
                //specificWarehouseInfo.Add(warehouse.Code);
                InventoryRecord inventoryRecord = _inventoryService.Get(currentContent.Code, warehouse.Code);

                // Have this also
                //var inventory = currentContent.GetStockPlacement(); // does "List"

                if (inventoryRecord == null) // means that the SKU is not referenced from that WH
                {
                    specificWarehouseInfo.Add(String.Format("WH: {0} - Available Qty: {1} ", warehouse.Code, 0));
                }
                else
                {
                    specificWarehouseInfo.Add(String.Format(
                        "WH: {0} - Available Qty: {1} - Req Qty: {2} "
                        , warehouse.Code
                        , inventoryRecord.PurchaseAvailableQuantity
                        , inventoryRecord.PurchaseRequestedQuantity));

                    // may need to look at Back & Pre - qty
                    specificWarehouseInfo.Add(String.Format(
                        "BackOrderAvailable Qty: {0} - BackOrder Req Qty: {1} "
                         , inventoryRecord.BackorderAvailableQuantity
                        , inventoryRecord.BackorderRequestedQuantity));

                    requestedQuantity += inventoryRecord.PurchaseRequestedQuantity;
                }
            }
            generalWarehouseInfo.Add(String.Format("Total req qty: {0}", requestedQuantity)); // it adds up across WHs
        }

        //Injected<IOrderGroupFactory> _ogf;
        public ActionResult RequestInventory(string code) // step through this
        {
            // from Fund, maybe not what we want - heve more in Tran-Scope in CheckOutController
            //IOrderGroup c = _orderRepository.Create<ICart>(""); // 
            //c.AdjustInventoryOrRemoveLineItems();
            //c.UpdateInventoryOrRemoveLineItems()

            // Get the WH
            string warehouseCode = "Nashua"; // a bit hard-coded
            //string warehouseCode = "Stocholm"; // a bit hard-coded here too

            // somewhat hard-coded...  no "Loader" though
            // could of course get info from the Market--> Country --> City etc.
            string warehouseCode2 = _warehouseRepository.List()
                 .Where(w => w.ContactInformation.City == "Nashua")
                 .First().Code;

            // can get several, should have some Geo-LookUp or alike to get the nearest WH
            IEnumerable<string> www = GetLocalMarketWarehouses();

            // Get the actual record in focus
            InventoryRecord inventoryRecord = _inventoryService.Get(code, warehouseCode);

            // could have some decisions made by the following props... just havinga look here
            decimal available = inventoryRecord.PurchaseAvailableQuantity;
            decimal requested = inventoryRecord.PurchaseRequestedQuantity;
            decimal backOrderAvailable = inventoryRecord.BackorderAvailableQuantity;
            decimal backOrderRequested = inventoryRecord.BackorderRequestedQuantity;

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem(); // The one we use now
            requestItem.CatalogEntryCode = code;
            requestItem.Quantity = 3M;
            requestItem.WarehouseCode = warehouseCode;
            // ...no time-out ootb --> custom coding... like for consert tickets

            // calls for some logic
            requestItem.RequestType = InventoryRequestType.Purchase; // reserve for now

            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest =
                new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = _inventoryService.Request(inventoryRequest);
            // pseudo-code for the "key" below
            //requestItem.OperationKey = requestItem.WarehouseCode + requestItem.CatalogEntryCode + requestItem.Quantity.ToString();

            if (inventoryResponse.IsSuccess)
            {
                TempData["key"] = inventoryResponse.Items[0].OperationKey; // bad place, just for demo
                // Storage is prepared in [dbo].[Shipment] ... or done "custom"
                // [OperationKeys] NVARCHAR (MAX)   NULL,
                // methods for management sits on the Shipment
                //      Serialized, InMemory and OldSchool
            }
            else
            {
                // could start to adjust Pre/Back-Order-qty
                InventoryRequestItem backOrderRequestItem = new InventoryRequestItem();
                backOrderRequestItem.CatalogEntryCode = code;
                backOrderRequestItem.Quantity = 3M;
                backOrderRequestItem.WarehouseCode = warehouseCode;
                backOrderRequestItem.RequestType = InventoryRequestType.Backorder; // ...if enabled
                //Metadata below
                //backOrderRequestItem.OperationKey = backOrderRequestItem.RequestType + backOrderRequestItem.WarehouseCode + backOrderRequestItem.CatalogEntryCode + backOrderRequestItem.Quantity.ToString();

                List<InventoryRequestItem> backOrderRequestItems = new List<InventoryRequestItem>(); // holds the "items"    
                backOrderRequestItems.Add(backOrderRequestItem);

                InventoryRequest backOrderRequest = new InventoryRequest(DateTime.UtcNow, backOrderRequestItems, null);

                InventoryResponse backOrderInventoryResponse =
                    _inventoryService.Request(backOrderRequest);
            }

            // dbo.InventoryService - table gets the requests and accumulate
            // inventoryService.Service.Request(requestItem);
            // and it increases in reserved until you can´t reserve more --> "Not Success"

            return RedirectToAction("Index", "Variation");
        }

        public ActionResult CancelRequest(string code)
        {
            // use the "key" ... "WH-location", entry & Qty are irrelevant as the "key" governs what has happend
            // all are overlooked even if entered

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem();

            // calls for some logic
            requestItem.RequestType = InventoryRequestType.Cancel; // as a demo
            requestItem.OperationKey = TempData["key"] as string;

            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest = new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = _inventoryService.Request(inventoryRequest);

            return RedirectToAction("Index", "Variation");

            // Check the "Complete-method" 
            // ...no time-limited reservation 
            // ......could do a work-around with a timer counting down... and then cancel in code
            // cmd-app has a custom implementation of "the provider" (CustomInventoryService.cs)
        }

        public ActionResult SetInventory(VariationContent currentContent)
        {
            _inventoryService.Insert(new[] // need an initial one...
            {
                new InventoryRecord
                {
                    AdditionalQuantity = 0, // backward comp. --> means Reserved
                    BackorderAvailableQuantity = 0,
                    BackorderAvailableUtc = DateTime.UtcNow,
                    CatalogEntryCode = currentContent.Code,
                    IsTracked = true,
                    WarehouseCode = "Nashua",
                    PreorderAvailableQuantity = 0,
                    PreorderAvailableUtc = DateTime.UtcNow,
                    PurchaseAvailableQuantity = 100,
                    PurchaseAvailableUtc = DateTime.UtcNow
                }
            });

            return RedirectToAction("Index", "Variation");
        }

        public ActionResult UpdateInventory(VariationContent currentContent)
        {
            InventoryChange theChange = new InventoryChange()
            {
                CatalogEntryCode = currentContent.Code,
                WarehouseCode = "Nashua",
                PurchaseAvailableChange = -4
            };

            _inventoryService.Adjust(theChange);

            /*===*/

            _inventoryService.Update(new[]
             {
                 new InventoryRecord
                  {
                      CatalogEntryCode = currentContent.Code,
                      PurchaseAvailableQuantity = 199,
                      PurchaseAvailableUtc = DateTime.UtcNow,
                      WarehouseCode = "Nashua"
                  }
             });

            return RedirectToAction("Index", "Variation");
        }

        #endregion

        #region Associations

        private Dictionary<string, ContentReference> GetAggregatedAssocciations(EntryContentBase currentContent)
        {
            // should return a chunk for each Assicciation
            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();
            Dictionary<string, ContentReference> localStuff = new Dictionary<string, ContentReference>();

            if (assoc.Count() > 0)
            {
                foreach (var item in assoc)
                {
                    localStuff.Add(GetAssociationMetaDataSingle(item), GetAssociatedEntry(item)); // need to refactor the returntypes and the 
                    //localStuff.Add("Nothing", ContentReference.SelfReference); // just to some back... for now
                }
            }
            else
            {
                localStuff.Add("Nothing", ContentReference.SelfReference); // just to some back... for now
            }
            return localStuff;

            // may need theese (supporting classes)
            //AssociationModel otherModel = new AssociationModel();
            //List<AssociationModel> otherModels = new List<AssociationModel>();
        }

        private string GetAssociationMetaDataSingle(EPiServer.Commerce.Catalog.Linking.Association assoc)
        {
            // pops in "foreach"
            StringBuilder strB = new StringBuilder();

            strB.Append("Group-Name: " + assoc.Group.Name);
            strB.Append(" ");
            strB.Append("Group-Description: " + assoc.Group.Description);
            strB.Append(" ");
            strB.Append("Group-Sort: " + assoc.Group.SortOrder);
            strB.Append(" ");
            strB.Append("Type-Id: " + assoc.Type.Id); // where the filter could be applied
            strB.Append(" ");
            strB.Append("Type-Descr: " + assoc.Type.Description);
            // there is more to get out

            return strB.ToString();
        }

        private ContentReference GetAssociatedEntry(EPiServer.Commerce.Catalog.Linking.Association assoc)
        {
            return assoc.Target;
        }


        #endregion

        #region Find - BoughThisBoughtThat
        private IEnumerable<string> GetOtherEntries(string code)
        {
            if (IsOnLine) // quick fix for checking network
            {
                IClient client = Client.CreateFromConfig();
                FindQueries f = new FindQueries(client, true);
                return f.GetItems(code);
            }
            else
            {
                List<string> localList = new List<string>();
                localList.Add("not on-line");
                return localList;
            }
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using CommerceTraining.Models.Catalog;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce;
using EPiServer.Web.Routing;
using EPiServer.Commerce.Catalog;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Orders;
using EPiServer.Commerce.Catalog.ContentTypes;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using System;
using CommerceTraining.SupportingClasses;
using System.Text;
using EPiServer.Commerce.Catalog.Linking;
using Mediachase.Commerce.Catalog;
using CommerceTraining.Infrastructure.CartAndCheckout;
using EPiServer.Find;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Orders.Managers;

namespace CommerceTraining.Controllers
{
    public class TrousersController : CatalogControllerBase<TrousersVariation>
    {
        // ToDo: Improve the IsOnLine - stuff
        private static bool IsOnLine { get; set; } // QuickFix

        private readonly IPriceService _priceService;
        private readonly IPriceDetailService _priceDetailService;
        public static IContentLoader xyz;

        public TrousersController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver // use this in node listing instead
            , IPriceService priceService
            , IPriceDetailService pricedetailService
            , ICurrentMarket currentMarket
            )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver, currentMarket)
        {
            _priceService = priceService;
            _priceDetailService = pricedetailService;
            _currentMarket = currentMarket;
        }

        public ActionResult Index(TrousersVariation currentContent)
        {
            IsOnLine = CheckIfOnLine.IsInternetAvailable; // Need to know... for Find

            var model = new TrousersViewModel
            {
                MainBody = currentContent.MainBody, // boom if empty...
                priceString = currentContent.GetDefaultPrice().UnitPrice.Amount.ToString("C"),
                discountPrice = StoreHelper.GetDiscountPrice(currentContent.LoadEntry()),
                image = GetDefaultAsset(currentContent),
                CanBeMonogrammed = false,
                //ProductArea = currentContent.ProductArea,
                //CartUrl = cartUrl, // new style
                //WishlistUrl = wUrl, // new style

                // For Adv.
                // warehouse info
                generalWarehouseInfo = this.generalWarehouseInfo,
                specificWarehouseInfo = this.specificWarehouseInfo,
                entryCode = currentContent.Code,

                // Associations
                Associations = GetAssociatedEntries(currentContent), // IEnumerable<ContentReference> // Remove when Aggregation is done
                AssociationMetaData = GetAssociationMetaData(currentContent), // string // Remove when Aggregation is done
                AssocAggregated = GetAggregatedAssocciations(currentContent), // Dictionary<string, ContentReference> // The final thing

                // Searchendizing ... need to be OnLine to use
                BoughtThisBoughtThat = GetOtherEntries(currentContent.Code)
            };

            return View(model);
        }

        // CartAndCheckoutService - used below (ccService)
        public ActionResult AddToCart(TrousersVariation currentContent, decimal Quantity)
        {

            var cart = _orderRepository.Service.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");

            ILineItem li = _orderGroupFactory.Service.CreateLineItem(currentContent.Code, cart);
            li.Quantity = Quantity;

            cart.AddLineItem(li);

            ccService.CheckOnLineItem(currentContent, li);
            //ccService.IsSecondShipmentReqired(cart);
            ccService.CheckOnLineItems(cart);

            _orderRepository.Service.Save(cart);

            string passingValue = cart.Name; // if needed, send something along
            ContentReference cartRef = _contentLoader.Get<StartPage>(ContentReference.StartPage).Settings.cartPage;

            return RedirectToAction("Index", new { node = cartRef, routedData = passingValue });

        }

        // This (or most of it) should be moved to "ccService" and .ctor
        Injected<ReferenceConverter> _refConv;
        Injected<IOrderGroupFactory> _orderGroupFactory;
        Injected<IOrderRepository> _orderRepository;
        Injected<IPlacedPriceProcessor> _placedPriceProcessor;
        Injected<IInventoryProcessor> _inventoryProcessor;
        CartAndCheckoutService ccService = new CartAndCheckoutService(); // taking care of the "specials"

        public ActionResult AddTrousersAndSuspenders(TrousersVariation currentContent, string accessoryCode)
        {
            // ... it's still most hard-coded for demoing 
            // key thing here is the shipping parameters, CartController read the stuff properly
            accessoryCode = "Suspenders_1";

            var cart = _orderRepository.Service.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");

            // Here is what differs for serialized carts, we need to initially set a value
            //  (so the property is created)
            // matching property exist in MDP... and epi does a "name lookup" later on, 
            cart.Properties["SpecialShip"] = false;

            // The trousers... get a LineItem and send to ccService for possible alteration
            ILineItem lineItem1 = _orderGroupFactory.Service.CreateLineItem(currentContent.Code, cart);

            // ...can use something like this, it's the trousers, so no special shipping
            // Sets the "RequireSpecialShipping" (bool) to false... it's "object" by default in serialized mode)
            var someBool = ccService.CheckOnLineItem(currentContent, lineItem1);

            lineItem1.Quantity = 1; // ...it's just a demo

            _placedPriceProcessor.Service.UpdatePlacedPrice
                (lineItem1, GetContact(),  _currentMarket.GetCurrentMarket().MarketId, cart.Currency
                , (lineItemToValidate, ValidationIssue) => { }); //look in "Fund" for ValidationIssues

            // if okay, do this
            cart.AddLineItem(lineItem1);

            // Now, for the other Entry... the "Suspenders"
            ContentReference contRef = _refConv.Service.GetContentLink(accessoryCode);
            ClothesAccessory accessory = _contentLoader.Get<ClothesAccessory>(contRef);

            // a bit "un-flexible" here, but... we're just showing a demo
            ILineItem lineItem2 = _orderGroupFactory.Service.CreateLineItem(accessory.Code, cart);
            lineItem2.Quantity = 1; 

            // cannot use cart.AddLineItem(...); - it ends up on the first Shipment 
            //  ... so we create another shipment and add the second LI there
            // We now get a hit on the "special shipment" requirement
            if (ccService.CheckOnLineItem(accessory, lineItem2)) // Sets the "RequireSpecialShipping" (bool) to true)
            {
                //ccService.AddAnotherShipment(cart, lineItem2, false); // RoCe: fix this one, use later
                // ...adding manually here below

                IShipment newShipment = _orderGroupFactory.Service.CreateShipment(cart);
                cart.AddShipment(newShipment);

                _placedPriceProcessor.Service.UpdatePlacedPrice
                    (lineItem2, GetContact(), _currentMarket.GetCurrentMarket().MarketId, cart.Currency
                    , (lineItemToValidate, ValidationIssue) => { });

                // just adding another address here
                IOrderAddress newAddress = _orderGroupFactory.Service.CreateOrderAddress(cart);
                newAddress.City = "Kalmar";
                newAddress.CountryCode = "sv";
                newAddress.CountryName = "Sweden";
                newAddress.Id = "NewShipAddress"; // need to be here

                newShipment.ShippingAddress = newAddress;
                newShipment.LineItems.Add(lineItem2);

                newShipment.ShippingMethodId = ccService.CheckApplicableShippingsNew(lineItem2.Code);
                // or...
                var g = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false)
                    .ShippingMethodParameter.Where(s => s.Value == lineItem2.Code);

                // not so easy as "Methods"... have to use the "undocumented BF-stuff"
                //ShippingManager.GetShippingOption(new Guid(g.ToString())).ShippingOptionParameter.Where(p=>p.)

                // by default we get "Awaiting Inventory" and cannot release the shipment (in Com-Man)
                // not enough code in this part of the demo... for this
                newShipment.OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

                // with the below it gets released ... and we can only cancel it 
                // the below is not sufficient, will/should be done elsewhere
                _inventoryProcessor.Service.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                     , OrderStatus.Completed, (item, issue) => { }); // can have a look in "Fund"

                // just a check
                //var lis = cart.GetAllLineItems();

                /* Save ... */
                _orderRepository.Service.Save(cart);
            }

            string passingValue = cart.Name; // ... or send something else along
            ContentReference cartRef = _contentLoader.Get<StartPage>(ContentReference.StartPage).Settings.cartPage;

            return RedirectToAction("Index", new { node = cartRef, routedData = passingValue });
        }

        /* For Adv. below - Demo in shirt controller instead*/
        // ToDo: need to refactor and move this stuff to a separate class (duplicated now)

        #region Warehouse and Inventory, not used for the split-ship demo
        // demo of the following is done with the white shirt

        Injected<IWarehouseRepository> warehouseRepository;
        Injected<IInventoryService> inventoryService;

        List<string> generalWarehouseInfo = new List<string>();
        List<string> specificWarehouseInfo = new List<string>();

        // check what we have in stock
        private void CheckWarehouses(ShirtVariation currentContent)
        {
            generalWarehouseInfo.Add("Entry inventory Tracked: " + currentContent.TrackInventory.ToString());

            // other code in CommerceDemo\...\AdminPageTemplate

            // how many FFCenters
            IEnumerable<IWarehouse> fullfillmentCenters = warehouseRepository.Service.List()
                .Where(w => (currentContent.ApplicationId == w.ApplicationId.ToString()) && w.IsActive && w.IsFulfillmentCenter);

            if (fullfillmentCenters.Count() > 1)
            {
                generalWarehouseInfo.Add("More than one fullfillment centers, need custom logic");
            }

            // get all WHs for enumeration and output to the view
            IEnumerable<IWarehouse> allWarehouses = warehouseRepository.Service.List();
            Decimal requestedQuantity = 0M;

            // ...there is also an InventoryLoader
            foreach (var warehouse in allWarehouses)
            {
                //specificWarehouseInfo.Add(warehouse.Code);
                InventoryRecord inventoryRecord = inventoryService.Service.Get(currentContent.Code, warehouse.Code);

                // Nice extension
                //var inventory = currentContent.GetStockPlacement();

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

        public ActionResult RequestInventory(string code) // step through this
        {
            string warehouseCode = "Stockholm"; // a bit hard-coded
            //string warehouseCode = "Nashua"; // a bit hard-coded

            InventoryRecord inventoryRecord = inventoryService.Service.Get(code, warehouseCode);
            decimal available = inventoryRecord.PurchaseAvailableQuantity;
            decimal requested = inventoryRecord.PurchaseRequestedQuantity;
            decimal backOrderAvailable = inventoryRecord.BackorderAvailableQuantity;
            decimal backOrderRequested = inventoryRecord.BackorderRequestedQuantity;

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem();
            requestItem.CatalogEntryCode = code;
            requestItem.Quantity = 3M;
            // ...no time-out --> custom

            // calls for some logic
            requestItem.WarehouseCode = warehouseCode;
            requestItem.RequestType = InventoryRequestType.Purchase; // reserve for now

            // pseudo-code below
            //requestItem.OperationKey = requestItem.WarehouseCode + requestItem.CatalogEntryCode + requestItem.Quantity.ToString();

            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest = new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = inventoryService.Service.Request(inventoryRequest);

            if (inventoryResponse.IsSuccess)
            {
                TempData["key"] = inventoryResponse.Items[0].OperationKey; // bad place, just for demo
                // Storage in [dbo].[Shipment]  or "custom"
                // [OperationKeys] NVARCHAR (MAX)   NULL,
            }
            else
            {
                // could start to adjust Pre/Back-Order-qty
                InventoryRequestItem backOrderRequestItem = new InventoryRequestItem();
                backOrderRequestItem.CatalogEntryCode = code;
                backOrderRequestItem.Quantity = 3M;
                backOrderRequestItem.WarehouseCode = warehouseCode;
                backOrderRequestItem.RequestType = InventoryRequestType.Backorder; // ...if enabled
                backOrderRequestItem.OperationKey = backOrderRequestItem.RequestType + backOrderRequestItem.WarehouseCode + backOrderRequestItem.CatalogEntryCode + backOrderRequestItem.Quantity.ToString();

                List<InventoryRequestItem> backOrderRequestItems = new List<InventoryRequestItem>(); // holds the "items"    
                backOrderRequestItems.Add(backOrderRequestItem);

                InventoryRequest backOrderRequest = new InventoryRequest(DateTime.UtcNow, backOrderRequestItems, null);

                InventoryResponse backOrderInventoryResponse =
                    inventoryService.Service.Request(backOrderRequest); // 

            }

            // ...gets 
            //dbo.InventoryService - table gets the requests and accumulate

            //inventoryService.Service.Request(requestItem);
            // request it... 
            // and it increases in reserved until you can´t reserve more --> "!Success"
            // ...and decreases available

            return RedirectToAction("Index", "Variation");
        }

        public ActionResult CancelRequest(string code)
        {
            // use the "key" ... "WH-location", entry & Qty are irrelevant as the "key" governs what has happend
            // all are overlooked even if entered

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem();

            // calls for some logic
            requestItem.RequestType = InventoryRequestType.Cancel; // 
            requestItem.OperationKey = TempData["key"] as string;

            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest = new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = inventoryService.Service.Request(inventoryRequest);

            return RedirectToAction("Index", "Variation");

            // Check the "Complete-method" 
            // ...no time-limited reservation (allthough it´s a custom implementation of "the provider")
            // ......could do a work-around with a timer counting down... and then cancel in code

        }

        public ActionResult SetInventory(VariationContent currentContent)
        {
            inventoryService.Service.Insert(new[]
            {
                new InventoryRecord
                {
                    AdditionalQuantity = 0, // backward comp. --> Reserved
                    BackorderAvailableQuantity = 0,
                    BackorderAvailableUtc = DateTime.UtcNow,
                    CatalogEntryCode = currentContent.Code,
                    IsTracked = true,
                    PreorderAvailableQuantity = 0,
                    PreorderAvailableUtc = DateTime.UtcNow,
                    PurchaseAvailableQuantity = 100,
                    PurchaseAvailableUtc = DateTime.UtcNow,
                    WarehouseCode = "Nashua"
                }
            });


            return RedirectToAction("Index", "Variation");
        }

        public ActionResult UpdateInventory(VariationContent currentContent)
        {
            inventoryService.Service.Update(new[]
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
        // not used for the trousers

        private Dictionary<string, ContentReference> GetAggregatedAssocciations(EntryContentBase currentContent)
        {
            // cannot get to the ContentLink with @Html.
            // should return a chunk for each Assicciation
            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();
            Dictionary<string, ContentReference> localStuff = new Dictionary<string, ContentReference>();

            if (assoc.Count() > 0)
            {
                foreach (var item in assoc)
                {
                    localStuff.Add(GetAssociationMetaDataSingle(item), GetAssociatedEntry(item)); // need to refactor the returntypes and the 
                }
            }
            else
            {
                localStuff.Add("Nothing", ContentReference.SelfReference); // just to some back... for now
            }
            return localStuff;

            // may need theese
            AssociationModel otherModel = new AssociationModel();
            List<AssociationModel> otherModels = new List<AssociationModel>();


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
            strB.Append("Type-Descr: " + assoc.Type.Description);
            // there is more to get out

            return strB.ToString();
        }

        private ContentReference GetAssociatedEntry(EPiServer.Commerce.Catalog.Linking.Association assoc)
        {
            return assoc.Target;
        }

        private string GetAssociationMetaData(EntryContentBase currentContent)
        {
            // not good, find a better way than do this twice
            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();
            StringBuilder strB = new StringBuilder();

            if (assoc.Count() >= 1)
            {
                // Fix code and formatting, but it works
                EPiServer.Commerce.Catalog.Linking.Association a = assoc.FirstOrDefault(); // get the only one .. so far for test
                strB.Append("GroupName: " + a.Group.Name);
                strB.Append(" - ");
                strB.Append("GroupDescription: " + a.Group.Description); // doesn't show - investigate
                strB.Append(" - ");
                strB.Append("GroupSort: " + a.Group.SortOrder);
                strB.Append(" - ");
                strB.Append("TypeId: " + a.Type.Id); // where the filter could be applied
                strB.Append(" - ");
                strB.Append("TypeDescr: " + a.Type.Description);
                // there is more to get out
                ContentReference theRef = a.Target;
                strB.Append("...in TrousersController");

            }
            else
            {
                strB.Append("Nothing");
            }

            return strB.ToString();
        }

        // ToDo: clean up here ... have an Aggergation for this part ... could use as demo
        private IEnumerable<ContentReference> GetAssociatedEntries(EntryContentBase currentContent)
        {
            // using linksRep is gone
            IAssociationRepository _linksRep = ServiceLocator.Current.GetInstance<IAssociationRepository>();

            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> linksRepAssoc =
                _linksRep.GetAssociations(currentContent.ContentLink).Where(l => l.Group.Name == "CrossSell");
            // would like to be able to filter when calling, instead of .Where()

            // would like to get the metadata out ... like type and group... and probaly treat them differently
            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();

            List<ContentReference> refs = new List<ContentReference>();

            foreach (EPiServer.Commerce.Catalog.Linking.Association item in assoc)
            {
                refs.Add(item.Target);
            }

            return refs;
        }

        #endregion

        // For Find and custom indexing
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

    }
}
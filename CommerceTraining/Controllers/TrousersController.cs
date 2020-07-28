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
        private static bool IsOnLine { get; set; } // QuickFix

        private readonly IPriceService _priceService;
        private readonly IPriceDetailService _priceDetailService;
        //public static IContentLoader xyz;

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
                MainBody = currentContent.MainBody, 
                priceString = currentContent.GetDefaultPrice().UnitPrice.Amount.ToString("C"),
                image = GetDefaultAsset(currentContent),
                CanBeMonogrammed = false,
                
                // warehouse info
                generalWarehouseInfo = this.generalWarehouseInfo,
                specificWarehouseInfo = this.specificWarehouseInfo,
                entryCode = currentContent.Code,

                // Associations
                Associations = GetAssociatedEntries(currentContent), 
                AssociationMetaData = GetAssociationMetaData(currentContent),
                
                // Dictionary<string, ContentReference> // The final thing
                AssocAggregated = GetAggregatedAssocciations(currentContent), 

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
                newAddress.Id = "NewShipAddress"; // needs to be here

                newShipment.ShippingAddress = newAddress;
                newShipment.LineItems.Add(lineItem2);

                newShipment.ShippingMethodId = ccService.CheckApplicableShippingsNew(lineItem2.Code);
                // or...
                var g = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false)
                    .ShippingMethodParameter.Where(s => s.Value == lineItem2.Code);

                // By default we get "Awaiting Inventory" and cannot release the shipment (in Com-Man)
                // not enough code for this in here...
                newShipment.OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

                // with the below it gets released ... and we can only cancel it 
                // the below is not sufficient, will/should be done elsewhere
                _inventoryProcessor.Service.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                     , OrderStatus.Completed, (item, issue) => { }); // can have a look in "Fund"

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

        // ToDo: clean up here ... have an Aggregation for this part ... could use as demo
        Injected<IAssociationRepository> _assocRep;
        private IEnumerable<ContentReference> GetAssociatedEntries(EntryContentBase currentContent)
        {
            //IAssociationRepository _assocRep = ServiceLocator.Current.GetInstance<IAssociationRepository>();

            IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = 
                currentContent.GetAssociations().Where(l => l.Group.Name == "CrossSell");

            // just checking
            var a = assoc.First().Group;

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
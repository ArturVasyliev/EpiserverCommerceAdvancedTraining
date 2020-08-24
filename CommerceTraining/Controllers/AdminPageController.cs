using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using System.Configuration;
using EPiServer.Security;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using EPiServer.Data.Dynamic;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Core;
using EPiServer.Commerce.Catalog.Linking;
using Mediachase.Commerce.Catalog;
using SpecialFindClasses;
using EPiServer.Find;
using CommerceTraining.Infrastructure.Pricing;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce;
using EPiServer.Commerce.Catalog;
using EPiServer.Web.Routing;
using CommerceTraining.Models.ViewModels;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Customers;
using Mediachase.Data.Provider;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.Cache;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Markets;
using System.Globalization;
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Meta.Management;
using Mediachase.BusinessFoundation.Core;
using CommerceTraining.Models.Catalog;
using Mediachase.MetaDataPlus;
using System.Data;
using EPiServer.Commerce.Order;

namespace CommerceTraining.Controllers
{
    public class AdminPageController : PageController<AdminPage>
    {
        private readonly IPriceService _priceService;
        private readonly IPriceDetailService _priceDetailService;
        private readonly ICurrentMarket _currentMarket;
        private readonly IMarketService _marketService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentLoader _contentLoader;
        public readonly AssetUrlResolver _assetUrlResolver;
        public readonly UrlResolver _urlResolver;
        //public readonly ILinksRepository _linksRepository; // obsoleted in ECF 11
        public readonly IOrderRepository _orderRepository;
        protected readonly ICatalogSystem _catalogSystem;

        public AdminPageController(
              IPriceService priceService
            , IPriceDetailService priceDetailService
            , ICurrentMarket currentMarketService
            , IMarketService marketService
            , ReferenceConverter referenceConverter
            , IContentLoader contentLoader
            , AssetUrlResolver assetUrlResolver
            , UrlResolver urlResolver
            //, ILinksRepository linksRepository // obsoleted in ECF 11
            , IOrderRepository orderRepository
            , ICatalogSystem catalogSystem)
        {
            _priceService = priceService;
            _priceDetailService = priceDetailService;
            _currentMarket = currentMarketService;
            _marketService = marketService;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _assetUrlResolver = assetUrlResolver;
            _urlResolver = urlResolver;
            //_linksRepository = linksRepository;
            _orderRepository = orderRepository;
            _catalogSystem = catalogSystem;
        }

        Injected<ICatalogSystem> _cat;

        public ActionResult Index(AdminPage currentPage)
        {
            //Mediachase.MetaDataPlus.Configurator.MetaClass m
            //    = Mediachase.MetaDataPlus.Configurator.MetaClass.Load(
            //    MetaDataContext.DefaultCurrent, 37);

            //MetaObject.GetList(MetaDataContext.Instance
            //    , m);

            /*System.Data.SqlClient.SqlException
  HResult=0x80131904
  Message=Could not find stored procedure 'mdpsp_avto_CatalogNodeEx_Fashion_Node_List'.
  Source=.Net SqlClient Data Provider
  StackTrace:
<Cannot evaluate the exception stack trace>
*/

            int i = 0;
            //CheckRelations();

            //CheckPricesWithTypeAndCode();

            //CheckInCache(); // not much to see

            //CatalogEntryDto dto =_cat.Service.GetCatalogEntryDto(100);
            //IEnumerable<CatalogEntryDto.SalePriceRow> p;

            #region Order-Cert Version 9
            /*
            Cart cart = new Cart("s", new Guid());
            PurchaseOrder po = new PurchaseOrder(cart.CustomerId);
            var c = CustomerContext.Current.CurrentContact.Email;
            OrderAddress o = new OrderAddress();
            var e = o.Email;
            var id = cart.CustomerId;

            OrderContext.Current.GetPurchaseOrder(cart.CustomerId,27); // obsoleted
            PurchaseOrderManager.GetNewPaymentPlanFromCart(cart);

            var cls = cart.MetaClass;
            */
            #endregion

            //SetBfPermissions(); // undocumented... with comments here

            /* Associations*/
            AddAssociationGroups(); // For the CMS - part
            //AddAssociation(); // Using the CMS-way
            //AddLabAssociations(); // ok, have some comments here
            //CheckAssociations(); // looking at what we have... comments included
            //AddAssociationForTest(); // new school and old school... with comments

            //TestFind();

            //CheckForCert();

            #region Do Later

            //GetCartFromPO(); // do this later

            //CheckNewOrderSystem();
            //UpdateShippingMethods();

            //CatalogContext.MetaDataContext.UseCurrentThreadCulture = false;

            //bool b = ViewBag.IsOnLine;
            //bool b = (bool)TempData["IsOnLine"]; // okay... for one call

            //ref_conv.Service.GetContentLink()

            //ContentReference theImage = CheckCommerceMedia(code, out group, out sortOrder);
            //ImageModel imageModel = _contentLoader.Get<ImageModel>(theImage);
            //var cat = imageModel.Category; // not...
            //var descr = imageModel.imageDescription;

            #endregion

            string code = "Long-Sleeve-Shirt-White-Small_1";
            PricingService pricingService = new PricingService(_priceService, _currentMarket, _priceDetailService);

            // create a price (Optional exercise?)
            //pricingService.CreatePrice(code); // write an optional lab of this ...

            IEnumerable<IPriceValue> thePriceList = pricingService.GetPrices(code);

            string group = string.Empty;
            string sortOrder = string.Empty;

            var model = new AdminPageViewModel
            {
                price = thePriceList.FirstOrDefault().UnitPrice.Amount.ToString("C"),
                prices = thePriceList,
                //imageFromNamedGroup = _urlResolver.GetUrl(theImage),
                //imageInfo = "Description: " + descr + " --> Group: " + group + " --> SortOrder: " + sortOrder,
                //OrderValues = CheckOnFind()
            };

            return View(model);
        }

        //Injected<IMarketService> _marketService;

        #region CheckingStuff_ECF_12
        public void CheckingStuff_ECF_12()
        {
            var curr = SiteContext.Current.Currency;

            // deprecated
            //SiteContext.Current.SiteId 

            var lang = SiteContext.Current.LanguageName;

            var a = Mediachase.Commerce.Core.AppContext.Current.AzureCompatible;
            var v = Mediachase.Commerce.Core.AppContext.GetProductVersion();
            var app = Mediachase.Commerce.Core.AppContext.Current.GetApplicationDto();



        }


        #endregion

        #region Checking Price with Custom SaleCode and SaleTtype in a specific market
        // Had this as question on course, doesn't work in this site, had it in the prev installation
        // will get it in this prosject asap.
        // Had a B2b & a B2C market and used the SaleCode for different price lists in each market
        private void CheckPricesWithTypeAndCode()
        {
            string code = "SomeShirtVariation_1";

            ContentReference theContRef = _referenceConverter.GetContentLink(code);
            VariationContent theSKU = _contentLoader.Get<VariationContent>(theContRef);

            CatalogKey catKey = new CatalogKey(code);

            // 4 is Jurisdiction Group, adding SaleCode
            List<CustomerPricing> customerPricing = new List<CustomerPricing>();
            customerPricing.Add(new CustomerPricing((CustomerPricing.PriceType)4, "se"));

            IMarket theMarket = _marketService.GetMarket("B2B_Mark"); // Note: max 8 char

            // By the extension-method we get all prices... Qty becomes custom-dev
            var prices = theSKU.GetPrices(new MarketId("B2B_Mark")
                , new CustomerPricing((CustomerPricing.PriceType)4, "se"));

            // define a filter... and it should be good enough
            //var prices2 = _priceService.GetPrices(theMarket.MarketId, )
        }

        #endregion

        #region BF Permissions in 9+ - undocumented stuff
        //  (introduced late 8... I think)

        private const string Commerce = "EPiCommerce";
        private static readonly PermissionRepository PermissionRepository =
            ServiceLocator.Current.GetInstance<PermissionRepository>();


        #endregion

        // For "Adv." below
        #region Various short examples - DDS, IoC
        // Don't forget the StructureMapDependencyResolver
        Injected<IContentLoader> _loader;

        private void CheckIoC()
        {
            // how to NOT "do it" in MVC
            IContentRepository _rep = ServiceLocator.Current.GetInstance<IContentRepository>();

            var p1 = _rep.Get<StartPage>(ContentReference.StartPage);

            var p2 = _loader.Service.Get<StartPage>(ContentReference.StartPage);

            // current
            CartPage somePage = _rep.GetDefault<CartPage>(ContentReference.StartPage);

            // old
            CartPage someOtherPage = DataFactory.Instance.GetDefault<CartPage>(ContentReference.StartPage);

        }

        private void DdsStuff()
        {
            //EPiServer.Data.Dynamic.
            DynamicDataStore store = DynamicDataStoreFactory.Instance.CreateStore("MyStore", typeof(Comment));
            var theStore = DynamicDataStoreFactory.Instance.GetStore("MyStore");

            Comment someComment = new Comment { text = "Hello DDS" };

            theStore.Save(someComment);
            //theStore.
        }

        class Comment
        {
            public string text { get; set; }
        }

        #endregion

        #region Markets

        public void CreateMarket()
        {
            MarketImpl newMarket = new MarketImpl("UK");

            // languages need to be enabled (available) to be included
            CultureInfo languageEn = new CultureInfo("en");
            Currency currency = new Currency("GBP"); // currency code
            string country = "GBR"; // country code // GBR in CM

            newMarket.LanguagesCollection.Add(languageEn);
            newMarket.CountriesCollection.Add(country);
            newMarket.CurrenciesCollection.Add(currency);

            newMarket.DefaultCurrency = currency;
            newMarket.DefaultLanguage = languageEn;

            newMarket.MarketName = "UK_Demo";
            newMarket.IsEnabled = true;
            newMarket.MarketDescription = "Just demoing";
            newMarket.PricesIncludeTax = false; // ECF 12

            _marketService.CreateMarket(newMarket);
        }

        // just an example
        public void DeleteMarket() // arg "dm"
        {
            _marketService.DeleteMarket("Spain");
        }

        #endregion

        #region Warehouses
        Injected<IWarehouseRepository> warehouseRepository;
        Injected<IInventoryService> inventoryService;
        public void CreateWarehouses()
        {
            // probably done in an Init-module/setupscript, with a check... it´s just a lab here
            // ...add some logic for "exists" / "or not" 


            List<string> warehouseNames = new List<string> { "Stockholm", "Nashua", "London" };
            //var warehouseRepository = context.Locate.Advanced.GetInstance<IWarehouseRepository>();

            foreach (var item in warehouseNames)
            {
                var warehouse = new Warehouse()
                {
                    //ApplicationId = AppContext.Current.ApplicationId,
                    Code = item,
                    Name = item,
                    IsActive = true,
                    IsPrimary = false,
                    IsFulfillmentCenter = false,
                    IsPickupLocation = true,
                    IsDeliveryLocation = true
                };

                // a bunch of mandatory fields in CM on the Address tab that must be added
                WarehouseContactInformation info = new WarehouseContactInformation()
                {
                    // would maybe not be the same for all in this lab
                    City = item,
                    CountryCode = "sv",
                    CountryName = "Sweden",
                    Email = "bo@epi.com",
                    FirstName = "Bo",
                    LastName = "Ek",
                    Line1 = "Regeringsgatan 67",
                    PostalCode = "12345",
                    RegionName = "Uppland",
                };

                // if null ref. when saving the WH --> prob. ContactInformation missing... 
                warehouse.ContactInformation = info; // so we add it (SDK has been updated on this issue)

                warehouse.Created = DateTime.UtcNow;
                warehouse.Modified = DateTime.UtcNow;
                warehouse.CreatorId = "admin";
                warehouse.ModifierId = "admin";

                // ...supply the above and you´re good...
                // The only place so far where I have to do this Created/Modified
                //  ...should have been taken care of by epi ... not beeing a manual input
                // ...odd! ... and I did it as anonymous, just supplying the "admin" name
                // And...
                // the creator is probably optional ... as modifier is blank here and it works
                // SortOrder is not set if using CM, but gets 2147483647 if done like the above
                // Address goes into the same table --> redundant data
                // could file a "blemish" on this both API-wise and for the SDK


                // checking world-code...not working
                //var warehouse = new Warehouse()
                //{
                //    Code = "NY",
                //    Name = "New York store",
                //    IsActive = true,
                //    IsPrimary = false,
                //    IsFulfillmentCenter = false,
                //    IsPickupLocation = true,
                //    IsDeliveryLocation = true
                //};

                warehouseRepository.Service.Save(warehouse);
                // okay now... but...
                /*The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value.
                    The data for table-valued parameter "@Warehouse" doesn't conform to the table type of the parameter.
                    The statement has been terminated.*/
                // don't remember what that error was... test again

            } //uncomment after test

            /*Value cannot be null.
Parameter name: ContactInformation */
        }

        public void EditWarehouse()
        {
            // 6 - Test
            //var w = warehouseRepository.Service.Get("Test") as Warehouse; // get null
            //w.IsPickupLocation = true;
            //warehouseRepository.Service.Save(w);

            // from Bien
            var warehouse = warehouseRepository.Service.Get("test");
            //var writableCloneWarehouse = new Warehouse(warehouse);
            //writableCloneWarehouse.IsPickupLocation = true;
            //warehouseRepository.Service.Save(writableCloneWarehouse);

            var ir = inventoryService.Service.Get("dummy_1", warehouse.Code);
            //ir.AdditionalQuantity = 5; // will crash "Invalid use of read-only object"

            var ir2 = ir.CreateWritableClone();
            ir2.AdditionalQuantity = 5; // "reserved"
            var list = new List<InventoryRecord>
            {
                ir2
            };
            inventoryService.Service.Save(list);
        }

        // doing with the InventoryService
        public void CheckInventory()
        {
            string entryCode = "dummy_1";
            var warehouse = warehouseRepository.Service.Get("Test");
            var inventoryRecord = inventoryService.Service.Get(entryCode, warehouse.Code);

            decimal available = inventoryRecord.PurchaseAvailableQuantity;
            //decimal requested = inventoryRecord.AdditionalQuantity; // not used anymore
            decimal requested = inventoryRecord.PurchaseRequestedQuantity;

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem
            {
                CatalogEntryCode = entryCode,
                Quantity = 3M,
                WarehouseCode = warehouse.Code,
                RequestType = InventoryRequestType.Purchase
            };

            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest =
                new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = inventoryService.Service.Request(inventoryRequest);

            string theKey = String.Empty;

            if (inventoryResponse.IsSuccess)
            {
                theKey = inventoryResponse.Items[0].OperationKey;
                var info = inventoryResponse.Items.First().ResponseType;
                //ResponseTypeInfo ... seems not used
            }
            else
            {
                var info = inventoryResponse.Items.First().ResponseType;
            }

            //Checking
            var inventoryRecord2 = inventoryService.Service.Get(entryCode, warehouse.Code);
            decimal available2 = inventoryRecord2.PurchaseAvailableQuantity;
            decimal requested2 = inventoryRecord2.PurchaseRequestedQuantity;

            bool putBack = false;
            if (putBack)
            {
                List<InventoryRequestItem> requestItems2 = new List<InventoryRequestItem>(); // holds the "items"
                InventoryRequestItem requestItem2 = new InventoryRequestItem
                {
                    RequestType = InventoryRequestType.Cancel,
                    OperationKey = theKey
                };

                requestItems2.Add(requestItem2);

                InventoryRequest inventoryRequest2 = new InventoryRequest(DateTime.UtcNow, requestItems2, null);

                InventoryResponse inventoryResponse2 =
                    inventoryService.Service.Request(inventoryRequest2);

                // crash when all is turned off
                var info2 = inventoryResponse2.Items.First().ResponseType;
            }

            //Checking
            var inventoryRecord3 = inventoryService.Service.Get(entryCode, warehouse.Code);
            decimal available3 = inventoryRecord3.PurchaseAvailableQuantity;
            decimal requested3 = inventoryRecord3.PurchaseRequestedQuantity;

        }
        #endregion

        #region Taxes

        public void CreateTaxCategoryAndJurisdiction()
        {
            CatalogTaxDto t_Dto = CatalogTaxManager.CreateTaxCategory("VAT", true);
 
            JurisdictionDto jurisdictionDto = JurisdictionManager.GetJurisdictions(JurisdictionManager.JurisdictionType.Tax);

            JurisdictionDto.JurisdictionRow jurisdictionRow = jurisdictionDto.Jurisdiction.NewJurisdictionRow();
            jurisdictionRow.County = "HomeLand";
            jurisdictionRow.DisplayName = "HomeLand";
            jurisdictionRow.District = "WholeCountry";
            jurisdictionRow.CountryCode = "se";
            jurisdictionRow.Code = "se";
            jurisdictionRow.JurisdictionType = (int)JurisdictionManager.JurisdictionType.Tax;
            jurisdictionDto.Jurisdiction.AddJurisdictionRow(jurisdictionRow);

            JurisdictionDto.JurisdictionGroupRow jurisdictionGroup = jurisdictionDto.JurisdictionGroup.NewJurisdictionGroupRow();
            jurisdictionGroup.DisplayName = "HomeLand Group";
            jurisdictionGroup.Code = "se_gr";
            jurisdictionGroup.JurisdictionType = JurisdictionManager.JurisdictionType.Tax.GetHashCode();
            jurisdictionDto.JurisdictionGroup.AddJurisdictionGroupRow(jurisdictionGroup);

            JurisdictionDto.JurisdictionRelationRow jurisdictionRelation = jurisdictionDto.JurisdictionRelation.NewJurisdictionRelationRow();
            jurisdictionRelation.JurisdictionRow = jurisdictionRow;
            jurisdictionRelation.JurisdictionGroupRow = jurisdictionGroup;
            jurisdictionDto.JurisdictionRelation.AddJurisdictionRelationRow(jurisdictionRelation);

            JurisdictionManager.SaveJurisdiction(jurisdictionDto);
        }

        public void CreateTaxes()
        {
            TaxDto orderTaxDto = TaxManager.GetTaxDto(TaxType.SalesTax);

            TaxDto.TaxRow taxRow = orderTaxDto.Tax.AddTaxRow(TaxType.SalesTax.GetHashCode(), "HomeLand_VAT", 10);

            TaxDto.TaxValueRow taxValueRow = orderTaxDto.TaxValue.NewTaxValueRow();

            taxValueRow.TaxId = taxRow.TaxId;
            taxValueRow.JurisdictionGroupId = JurisdictionManager.GetJurisdictionGroup("se_gr").JurisdictionGroup[0].JurisdictionGroupId;
            taxValueRow.TaxCategory = "VAT";
            taxValueRow.Percentage = 25;
            taxValueRow.AffectiveDate = DateTime.UtcNow;
            orderTaxDto.TaxValue.AddTaxValueRow(taxValueRow);
            
            TaxManager.SaveTax(orderTaxDto);
        }

        // For test
        private static void GetTaxes()
        {
            string taxCategory = CatalogTaxManager.GetTaxCategoryNameById(1);
            TaxValue[] t = OrderContext.Current.GetTaxes(
                Guid.Empty
                //OrderConfiguration.Instance.ApplicationId // ignored 
                , taxCategory, "sv", "sv", null, null, null, null, null);

            //TaxManager.GetTaxes()
        }

        #endregion

        #region BF 

        // Demo Std. BF-API
        public void ReadData()
        {
            // ToDo: want to load the "ContactNotes" for a specific individual
            // A bit explicit, but it´s a demo
            // Only reading data, does not need the "data-model"

            // Loading a contact to get the ID
            IEnumerable<CustomerContact> c = CustomerContext.Current.GetContactsByPattern("admin");
            PrimaryKeyId contacId = (PrimaryKeyId)c.First().PrimaryKeyId;

            // Filter and Sort collections
            FilterElementCollection filterCol =
                new Mediachase.BusinessFoundation.Data.FilterElementCollection();
            SortingElementCollection sortCol =
                new Mediachase.BusinessFoundation.Data.SortingElementCollection();

            // filters and sort objects to use
            FilterElement filterOnUserId =
                Mediachase.BusinessFoundation.Data.FilterElement.EqualElement("ContactId", contacId);
            FilterElement filterOnNoteType =
                Mediachase.BusinessFoundation.Data.FilterElement.EqualElement("NoteTitle", "Complaint");

            // RoCe: need to change this
            SortingElement sort = SortingElement.Ascending("NoteType");

            // add filter and sort
            filterCol.Add(filterOnUserId);
            filterCol.Add(filterOnNoteType);
            sortCol.Add(sort);

            EntityObject[] entities =
                BusinessManager.List("ContactNote", filterCol.ToArray(), sortCol.ToArray());
        }

        // Demo
        public static void UpdateData() // 
        {

            // ...got this guid in some way
            PrimaryKeyId pk = (PrimaryKeyId)(new Guid("6d5a75c9-daae-49bd-8c06-c704874e253e"));
            // ...is John John
            // Load
            EntityObject TheGuy = BusinessManager.Load(CustomerContact.ClassName, pk);

            // Update
            TheGuy["LastName"] = "Johansson";
            BusinessManager.Update(TheGuy);

            //...the principle, if not using the "Update-method":
            LoadRequest request = new LoadRequest(new EntityObject(CustomerContact.ClassName, pk));
            LoadResponse response = (LoadResponse)BusinessManager.Execute(request);
            EntityObject TheSameGuy = response.EntityObject;

            TheSameGuy["FirstName"] = "Johan";
            UpdateRequest request2 = new UpdateRequest(TheSameGuy);
            Response response2 = BusinessManager.Execute(request2);

        }

        // Demo/exercise of altering the BF model, complemented with the two following methods
        public void CreateClubCard()
        {
            try
            {
                // A Business Meta Model have two modes, "RunTime" and "DesignTime". 
                // The Runtime mode is default state and allows for setting and retreving data from objects and properties
                // The DesignMode is for operations like edit, create and delete MetaModel classes and MetaFields

                // ToDo: Open for edit (aka."Design mode")
                // ... we also have DeleteMetaClass(), CreateMetaView() etc.
                using (MetaClassManagerEditScope myEditScope = DataContext.Current.MetaModel.BeginEdit())
                {
                    // Create a MetaClass, definitions of the ClubCard 
                    MetaClass myNewClass =
                        DataContext.Current.MetaModel.CreateMetaClass(
                            "ClubCard", "ClubCard", "ClubCards", "labClass_ClubCard"
                            , PrimaryKeyIdValueType.Guid);

                    // Check this and document in course manual
                    myNewClass.AccessLevel =
                        Mediachase.BusinessFoundation.Data.Meta.Management.AccessLevel.Customization;
                    // ...did not get rid of the icons with "development" ... but it depends on what kind of object

                    myEditScope.SaveChanges(); // Else Rollback 
                } // close the scope


                // Note: You are not able to edit the object in the UI yet, must set the Title property.
                // ...can add Fields via UI but not edit the object itself.
                // Before this is set an exception is thrown on a missing object... but no indication of what object is missing
                // It´s mentioned in the documentation that the Title field should be set


                // ToDo: Create fields on the class
                // ... can also do with CreateMetaField() 
                //   ...but the following is easier as MetaBuilder opens "Edit-Scope" Automatically and gives nice methods

                // having issues new style below this
                //using (MetaFieldBuilder myMetaBuilder =
                //    new MetaFieldBuilder
                //        (myMetaModelManager.MetaClasses["ClubCard"]))
                //{


                // New style

                using (MetaFieldBuilder myMetaBuilder =
                new MetaFieldBuilder
                    (DataContext.Current.MetaModel.MetaClasses["ClubCard"]))
                {
                    // Set Title so the UI can be used for edit and additional BF features 
                    MetaField theTitleField = myMetaBuilder.CreateText("TitleField", "TitleField", false, 100, false);

                    // Have to do the following...else crash when editing
                    myMetaBuilder.MetaClass.TitleFieldName = theTitleField.Name;

                    // Standard fields
                    myMetaBuilder.CreateText("CardOwnerName", "CardOwnerName", false, 100, false);
                    myMetaBuilder.CreateEmail("email", "email", false, 100, true);
                    myMetaBuilder.CreateInteger("Balance", "Balance Friendly", true, 0); // no fractions, collection of points

                    myMetaBuilder.SaveChanges();

                    // For clarity of this lab:
                    //  - an enum field is later created separately... in the method CreateEnumField()
                }

                // ToDo: Set a reference between Contact and ClubCard
                // ...when setting references to other classes, additional fields will be created by the system

                // Below is the one in "core"
                MetaField mf = MetaDataWrapper.CreateReference
                    ("Contact", "ClubCard", "ReferenceFieldName", "ReferenceFieldFriendlyName"
                    , false, "InfoBlock", "ClubCard", "10"); // Original: "Information"

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // ToDo: create enum (Gold, Silver and Bronze cart type) and assign to the ClubCard as a field
        public void CreateEnumField()
        {
            Mediachase.BusinessFoundation.Data.Meta.Management.MetaClassManager myMetaModelManager =
                Mediachase.BusinessFoundation.Data.DataContext.Current.MetaModel;

            // Create the enum 
            MetaFieldType myMetaEnum = null; //
            using (MetaClassManagerEditScope myEditScope =
                 Mediachase.BusinessFoundation.Data.DataContext.Current.MetaModel.BeginEdit())
            {
                myMetaEnum = MetaEnum.Create("CardType", "CardType", false);
                myMetaEnum.AccessLevel = Mediachase.BusinessFoundation.Data.Meta.Management.AccessLevel.System;
                myEditScope.SaveChanges(); // Else Rollback 
            }

            // Add the enum to the ClubCard as a field
            MetaField metaField = null; // 
            using (MetaFieldBuilder myMetaBuilder =
                new MetaFieldBuilder
            (myMetaModelManager.MetaClasses["ClubCard"]))
            {
                metaField = myMetaBuilder.CreateEnumField("CardTypeEnum", "CardTypeEnum", myMetaEnum.Name
                    , true, String.Empty, true);
                metaField.AccessLevel = Mediachase.BusinessFoundation.Data.Meta.Management.AccessLevel.System;
                myMetaBuilder.SaveChanges();
            }

            // add values to the enum
            MetaEnum.AddItem(metaField.GetMetaType(), "Gold", 1);
            MetaEnum.AddItem(metaField.GetMetaType(), "Silver", 2);
            MetaEnum.AddItem(metaField.GetMetaType(), "Bronze", 3);
        }

        // ToDo: Give a ClubCard to the "admin" Contact (if not done in the AccountPage)
        public void GiveClubCardToAdmin()
        {
            // Get the customer ... could  use the CurrentContact
            FilterElementCollection fc = new FilterElementCollection();
            fc.Add(FilterElement.EqualElement("FirstName", "admin"));

            // Don´t need sorting for this task, but sorting could come in handy elsewhere
            // A bit explicit loading if the customer below, but for step-by-step demo/lab it´s okay
            EntityObject[] theList = BusinessManager.List
                (CustomerContact.ClassName, fc.ToArray()); // null is sorting, ints are "Start-Count"

            EntityObject entity = theList[0];
            PrimaryKeyId pk = (PrimaryKeyId)entity.PrimaryKeyId; // nullable

            // Get a ClubCard definition and fill up with data
            EntityObject clubCard = BusinessManager.InitializeEntity("ClubCard");

            clubCard["CardTypeEnum"] = 1; // Can do like below if preferred
            //int value = MetaDataWrapper.GetEnumByName("CardType")
            //    .EnumItems.ToList().Where(e => e.Name == "Silver").FirstOrDefault().Handle;

            // Set the reference
            clubCard["ReferenceFieldNameId"] = pk; // The PK of the Contact

            // Other properties
            clubCard["FieldName"] = "AdminGuy";
            clubCard["TitleField"] = "AdminClubCard";
            clubCard["CardOwnerName"] = entity["FullName"];
            clubCard["email"] = entity["Email"]; // Case sensetive, so entity[email]... not good
            // For the Balance property we make use of the defined default at the field... so,  it´s not set here

            // Persist to DB
            PrimaryKeyId CcPk = BusinessManager.Create(clubCard);
        }

        // Completely un-documented, have to do this
        private void SetBfPermissions()
        {
            //  metaClass.Name.ToLower() ... for the clubcard 

            foreach (var permission in new[] { "list", "create", "edit", "delete", "view" })
            {
                var permissionType = new PermissionType(Commerce, String.Format("businessfoundation:{0}:{1}:permission", "clubcard", permission));
                if (PermissionRepository.GetPermissions(permissionType).All(x => x.EntityType != SecurityEntityType.Role))
                {
                    PermissionRepository.SavePermissions(permissionType, new[] { new SecurityEntity("Administrators", SecurityEntityType.Role) });
                }
            }
        }

        //Injected<IOrderRepository> _orderRepo;
        // Another not documented "feature" that could come in handy
        public void GetPoData() // Fetch orders by specific LineItems
        {
            // RoCe: fix this - it's not complete
            // Not the official way, and kind of hacky ... but it works :)
            //DataContext ctx = GetBfContext(); // cmd
            string lineItem = "Long Sleeve Shirt White Small_1"; // Code

            List<FilterElement> filters = new List<FilterElement>();
            filters.Add(new FilterElement("CatalogEntryId", FilterElementType.Equal, lineItem)); // the Code
            // could add a Date-filter on the LineitemEx or search for orderdates (and take it "backwards")

            List<PurchaseOrder> POs = new List<PurchaseOrder>(); // takes time to load a bunch or orders
            List<int> ints = new List<int>(); // Faster, mey be sufficent

            int PoNo = 0;
            Dictionary<int, decimal> dict = new Dictionary<int, decimal>();

            using (IDataReader reader = Mediachase.BusinessFoundation.Data.DataHelper.List("LineItem", filters.ToArray()))
            {
                // need the FeatureSwitch when going for POs
                while (reader.Read())
                {
                    //POs.Add(OrderContext.Current.GetPurchaseOrderById((int)reader["OrderGroupId"]));
                    //OrderContext.Current.get
                    ints.Add((int)reader["OrderGroupId"]);

                    //dict.Add((int)reader["OrderGroupId"], (decimal)reader["PlacedPrice"]);
                    // The DateTime, the int + custom fields are in another table, LineItemEx 
                }
                PoNo = ints.FirstOrDefault();
            }

            // oldSchool
            var po = OrderContext.Current.GetPurchaseOrderById(PoNo);
            // newSchool
            IPurchaseOrder po2 = _orderRepository.Load<IPurchaseOrder>(PoNo);

        }

        #endregion

        #region Associations 

        // World/SDK-code is older than old-school
        // Nice UI in CMS for this and CM still works... but we need to do it in code

        public void AddAssociationGroups()
        {
            // a "Default" we get for free, "Global groups" sits in DDS

            // note: ...shouldn't do like this, using the ServiceLocator.Current ...
            var associationDefinitionRepository =
                ServiceLocator.Current.GetInstance<GroupDefinitionRepository
                <AssociationGroupDefinition>>();

            // if those groups already exist, nothing happens 
            associationDefinitionRepository.Add(new AssociationGroupDefinition
            { Name = "CrossSell" });

            associationDefinitionRepository.Add(new AssociationGroupDefinition
            { Name = "Replacement" });

            associationDefinitionRepository.Add(new AssociationGroupDefinition
            { Name = "UpSell" });

            associationDefinitionRepository.Add(new AssociationGroupDefinition
            { Name = "Optional" });

            associationDefinitionRepository.Add(new AssociationGroupDefinition
            { Name = "Required" });

            // List what we have
            var associations = associationDefinitionRepository.List();

            // Commerce Manager is filtered (look at the Shirt-Prod)
            // Associations were/is set per Entry, now kind of global... 
            // ... DDS is read only by the CMS UI

            // below the new stuff (in CMS) it´s the same mess as it has allways been :)
            // Somekind of sync needs probably to be maintained between the Groups in DDS and ECF-DB
            // ... if using non UI-things

            // ServiceAPI also has some for Associations

            // GroupDefinition - Id & Name ...there is a "description" in CM
            // the actual assoc have a "Type" (shows up in CM, not in CMS-UI)
            // "Group" is what shows in CMS-UI (called Type)... and no Description
        }

        //Injected<ILinksRepository> linkRep;
        //Injected<ReferenceConverter> ref_conv;
        Injected<IAssociationRepository> _assocRep;
        public void AddAssociation() // the CMS-way
        {
            ContentReference sourceLink = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            ContentReference targetLink = _referenceConverter.GetContentLink("Long-Sleeve-Shirt-Blue-Medium_1");

            EPiServer.Commerce.Catalog.Linking.Association association =
                new EPiServer.Commerce.Catalog.Linking.Association
                {
                    Source = sourceLink,
                    Target = targetLink,
                    SortOrder = -10,
                    Type = new AssociationType() { Id = "Cool", Description = "NiceToHave" },
                    //association.Type = new AssociationType() { Id = "Buttons", Description = "For suspenders" };
                    //association.Type = new AssociationType() { Id = "Clips", Description = "For suspenders" };
                    //association.Group = new AssociationGroup() { Name = "Accessories", Description = "Spice up a customer", SortOrder = 0 };
                    Group = new AssociationGroup() { Name = "CrossSell", Description = "Give a customer more choices", SortOrder = -10 }
                };
            //association.Group = new AssociationGroup() { Name = "UpSell", Description = "Give a happier customer", SortOrder = 0 };

            // in 10 
            //_linksRepository.UpdateAssociation(association); // Note: there is no "Save..."

            // in 11
            _assocRep.Service.UpdateAssociation(association);


            // API-"Group" is named "Type" in the CMS-UI (maybe also in ServiceAPI)...check that
        }

        // Old-school, New-school
        public void AddAssociationForTest()
        {
            ContentReference sourceLink =
                _referenceConverter.GetContentLink("Trousers-with-buttons_1");
            ContentReference targetLink =
                _referenceConverter.GetContentLink("Galoshes_1");

            // Do it the CMS-way
            EPiServer.Commerce.Catalog.Linking.Association association =
                new EPiServer.Commerce.Catalog.Linking.Association
                {
                    Source = sourceLink,
                    Target = targetLink,
                    SortOrder = 15,

                    Type = new AssociationType()
                    {
                        Id = "Buttons trousers"
                        ,
                        Description = "Galoshes for button trousers"
                    }, // NO "Sort" for types

                    Group = new AssociationGroup()
                    {
                        Name = "OutdoorAccessories"
                        ,
                        Description = "For bad weather",
                        SortOrder = 20
                    }
                };

            //_linksRepository.UpdateAssociation(association); // 10
            // Note: We're using Injected<> to get the _assocRep, should be by .ctor instead
            _assocRep.Service.UpdateAssociation(association); // 11

            // Do it low-level using DTOs, first we get a bucket for associations, -1 means an empty one
            CatalogAssociationDto dto = _catalogSystem.GetCatalogAssociationDto(-1); 
            //CatalogContext.Current.GetCatalogAssociationDto(-1); 

            CatalogAssociationDto.AssociationTypeRow typeRow = dto.AssociationType.NewAssociationTypeRow();
            typeRow.AssociationTypeId = "BadBusiness";
            typeRow.Description = "Have to change manager";

            dto.AssociationType.AddAssociationTypeRow(typeRow);
            // This is what we have

            // ...need to do this before saving "the rest"
            CatalogContext.Current.SaveAssociationType(dto); 
            // This is kind of odd, does not follow the DTO-pattern
            // I found the below comment in the Mediachase.Commerce.Catalog.Data.CatalogAssociationAdmin
            // "Saves changes to the current DTO's associations. Changes to AssociationType are not saved."

            CatalogAssociationDto.CatalogAssociationRow associationRow =
                dto.CatalogAssociation.NewCatalogAssociationRow();
            associationRow.AssociationDescription = "This is the Association Description";
            associationRow.AssociationName = "HappySelling";
            associationRow.CatalogEntryId = _referenceConverter.GetObjectId(targetLink);
            associationRow.SortOrder = 100; // Mandatory here in code
            
            // have to add it to the DTO
            dto.CatalogAssociation.AddCatalogAssociationRow(associationRow);

            CatalogAssociationDto.CatalogEntryAssociationRow entryRow =
                dto.CatalogEntryAssociation.NewCatalogEntryAssociationRow();
            entryRow.AssociationTypeId = typeRow.AssociationTypeId;
            entryRow.CatalogEntryId = _referenceConverter.GetObjectId(sourceLink);
            entryRow.SortOrder = 99; // mandatory
            entryRow.CatalogAssociationId = associationRow.CatalogAssociationId;
            dto.CatalogEntryAssociation.AddCatalogEntryAssociationRow(entryRow);

            // and now it's time to "Save"
            _catalogSystem.SaveCatalogAssociation(dto);
            //CatalogContext.Current.SaveCatalogAssociation(dto); 
            // Note: look above... at the saving of "Association-type"... else
            /*The INSERT statement conflicted with the FOREIGN KEY constraint "FK_CatalogEntryAssociation_AssociationType". 
           * ..., table "dbo.AssociationType", column 'AssociationTypeId'.
              The statement has been terminated.*/
        }

        // make a lab about the below 
        public void AddLabAssociations()
        {
            // Kind of hardcoded but it illustrates how it works...and it´s a lab
            // should be parameterized

            ContentReference sourceLink = _referenceConverter.GetContentLink("Trousers-with-buttons_1");
            ContentReference targetLink = _referenceConverter.GetContentLink("Suspenders_1");

            EPiServer.Commerce.Catalog.Linking.Association association =
                new EPiServer.Commerce.Catalog.Linking.Association
                {
                    Source = sourceLink,
                    Target = targetLink,
                    SortOrder = 10,
                    Type = new AssociationType() { Id = "Buttons", Description = "For suspenders" },
                    Group = new AssociationGroup() { Name = "Accessories", Description = "Spice up a customer", SortOrder = 0 }
                };

            //_linksRepository.UpdateAssociation(association); // 10
            _assocRep.Service.UpdateAssociation(association); // 11

            /* If using the old stuff - sync is needed to the new stuff */
            /* Nothing about the AssociationType or the SortOrder in CMS UI*/
            /* ...have the Type in CM ... it´s the Description that shows and we see the correct SortOrder*/
            /* AssociationGroup-Description doesn't get into CMS... when saved */
            /* ... the Description can be reached from CM */
            /* Do it old-school and the Description gets persisted */

            /* AssociationGroupDefinition (DDS) missmatch slightly compared to AssociationGroup (ILinksRep)*/

            /* CM does not reach the "global ones" in DDS */

        }

        public void CheckAssociations()
        {
            var associationDefinitionRepository =
                ServiceLocator.Current.GetInstance<GroupDefinitionRepository<AssociationGroupDefinition>>();
            var associations = associationDefinitionRepository.List();
            // no Accessories in there ... but it pops up in CMS-UI...?
            // ...means that we read in both places for the dropdown in CMS-UI

            // Would like to have the AssociationGroups in CMS-Admin-Settings with the Description-prop
            // ...and labeled right

            // linkRep.Service ... no listing here

            CatalogAssociationDto dto = CatalogContext.Current.GetCatalogAssociationDto("Accessories");
            IEnumerable<CatalogAssociationDto.AssociationTypeRow> tRows = dto.AssociationType;
            IEnumerable<CatalogAssociationDto.CatalogAssociationRow> aRows = dto.CatalogAssociation;
            IEnumerable<CatalogAssociationDto.CatalogEntryAssociationRow> eRows = dto.CatalogEntryAssociation;
        }

        #endregion

        #region Relations
        public void CheckRelations()
        {
            #region scope

            // this was first checked in 8.16 / 8.17
            // Now it's time for 10+ and 11
            // ...some fishy stuff here in 10 ... the "Category" ... gives the object itself
            // looks like there is an "interceptor" here

            #endregion

            #region Doing som tests

            EntryContentBase entryWithProduct =
                _contentLoader.Get<EntryContentBase>(
                 _referenceConverter.GetContentLink("Long-Sleeve-Shirt-White-Small_1"));

            EntryContentBase entryWithoutProduct =
                _contentLoader.Get<EntryContentBase>(
                    _referenceConverter.GetContentLink("Gul_1"));

            EntryContentBase entryAsProduct =
                _contentLoader.Get<EntryContentBase>(
                    _referenceConverter.GetContentLink("Long-Sleeve-Shirt_1"));

            // is there a Parent.Parent...?
            // 10+ gives false/null on all using TryGet + out (did not in 8)
            // 10+ gives node for all 3 as parent (I think 9 did not, 8 gave tutti-frutti)

            var stuff0 = _loader.Service.Get<CatalogContentBase>(
                entryWithProduct.ParentLink); // could use this

            var stuff00 = _loader.Service.Get<CatalogContentBase>(
                entryWithoutProduct.ParentLink); // could use this

            var stuff000 = _loader.Service.Get<CatalogContentBase>(
                entryAsProduct.ParentLink); // could use this

            // this one only have one node
            var categories = entryAsProduct.GetCategories(); //Gets ContentRefs
            Categories categories1 = entryAsProduct.Categories; // Now a CatalogContent (CategoryProxy)... or?
            var category = _loader.Service.Get<CatalogContentBase>(
                categories1.ContentLink); // gets a ShirtProductProxy..?? "Long_Sleeve_Shirt"

            ContentReference theNodeRef = _referenceConverter.GetContentLink("Men_1");

            entryWithoutProduct.ParentLink = theNodeRef;

            NodeEntryRelation theRel = new NodeEntryRelation();
            theRel.IsPrimary = true; // came in 11.2

            //var cat = theOut00.Categories; // seems to be CMS-categories... prev 
            // ... error if created in the node directly
            // no error if done at the product

            // this one have 2 nodes
            var nodes = entryWithoutProduct.GetNodeRelations(); // still "Relations"
            var nodes2 = entryWithoutProduct.GetCategories(); // Gets ContentRefs
            var categories2 = entryWithoutProduct.Categories;
            var nodes3 = _loader.Service.Get<CatalogContentBase>(categories2.ContentLink);
            // gets a ShirtVariationProxy...?? the entry itself "Gul"

            var parents = entryWithProduct.GetParentEntries();
            // Get ContentRefs, have also extensions for getting Packages, Bundles and Products 

            //entryWithProduct.get

            #endregion

            #region epilogue

            // Changes in 10 ... not at all as before...
            // Changed i 9 (triggers twice now, not four times as in 8)
            // ...see the ReadMe.txt in NumberNine ECF-8.17 for more info...
            // 1073741825, SomeNode is "TheParentNode"... gets that when "outside" of a product
            // Need to walk up the hierarchy to find the node ... when "doing it" on a Product

            #endregion
        }

        #endregion

        #region Shipping 

        // No Demo - just for testing and checks... demo in website, not here
        public void CheckForShippingOptions() // 
        {
            IMarket market = _marketService.GetMarket(new MarketId("sv"));

            // get the rows
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(market.MarketId.Value, false);
            var methods = dto.ShippingMethod.Count;

            //var shippingOptions = dto.ShippingOption; // the Gateways

            // separate table - points to the Gateway
            var shippingOptionParam = dto.ShippingOptionParameter;

            Guid shippingOptionGuid = new Guid(); // for the Shipping Gateway
            string shippingName = String.Empty; // could be handy

            // look for params (separate table) ... gets the gateway guid
            foreach (var item in shippingOptionParam)
            {
                if (item.Parameter == "Suspenders") // ...the string is an illustration
                {
                    shippingOptionGuid = item.ShippingOptionId; // ShippingGateway ... need the id of RoyalMail set when created
                    shippingName = item.Value;
                }
            }

            ShippingMethodDto.ShippingMethodRow foundShipping = null;

            foundShipping = ShippingManager.GetShippingMethod(shippingOptionGuid).ShippingMethod.First();

            // could be furter granular eg. what type of method for the Gateway
            ShippingMethodDto.ShippingMethodParameterRow[] paramRows = foundShipping.GetShippingMethodParameterRows();

            if (paramRows.Count() != 0)
            {
                foreach (var item2 in paramRows) // could be here we can match lineItem with ...
                {
                    var p = item2.Parameter;
                    var v = item2.Value;
                }
            }
        }

        /* -- Use as demo -- */
        // ToDo: create a lab for this + use as demo
        public void CreateShippingOptions() // arg: "so"
        {
            /* Scope is to use the parameters for options & methods
               This we do for having a look if there is a "forced" shipping for a SKU
               ...and there is...as a case for an exercise and demo
               You could of course do/create this lookup in many custom ways, 
               but the goal is to use something already in place in Commerce */

            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false); // ...just using the "Default Market" as an example
            // Have the dbo.MarketShippingMethods table if we want to investigate further

            #region ...just checking / info

            // ShippingOption = Provider (with its Guid)
            // ShippingMethod has it's own guid ... and point to the Provider (the optionID-guid)
            // ShippingMethodDto.ShippingOptionParameterRow sop = dto.ShippingOptionParameter.FindByShippingOptionParameterId(1);

            #endregion

            // A new Gateway (called "Option" in API)
            ShippingMethodDto.ShippingOptionRow newShippingOption = null;

            bool newOption = false; // just for toggling... for test in here
            if (newOption)
            {
                // Using the "GenericGateway" in this demo, could have a custom one
                newShippingOption = dto.ShippingOption.AddShippingOptionRow // the Guid is used for methods and options
                    (Guid.NewGuid(), "RoyalMail", "Gateway for UK", "RoyalMail"
                    , "Mediachase.Commerce.Plugins.Shipping.Generic.GenericGateway, Mediachase.Commerce.Plugins.Shipping"
                    , DateTime.UtcNow, DateTime.UtcNow);
            }

            // New method
            ShippingMethodDto.ShippingMethodRow newShippingMethod = null;

            bool newMethod = false; // just for toggling... for test in here
            if (newMethod)
            {
                newShippingMethod = dto.ShippingMethod.AddShippingMethodRow(Guid.NewGuid()
                    , newShippingOption, "en", true, "RoyalMail"
                    , "Method on UK territory"
                    , 1, "usd", "RoyalMail", false, 100, DateTime.UtcNow, DateTime.UtcNow);

                // Set the market, kind of separate
                dto.MarketShippingMethods.AddMarketShippingMethodsRow(MarketId.Default.Value, newShippingMethod);
            }

            // New gateway options
            ShippingMethodDto.ShippingOptionParameterRow optionRow = null;

            bool newOptionParameter = false; // just for toggling here
            if (newOptionParameter)
            {
                optionRow = dto.ShippingOptionParameter.AddShippingOptionParameterRow(
                    newShippingOption, "Suspenders", "RoyalMail");
            }

            // Method options, could have further granularity
            bool newMethodParam = false; // just for toggling here
            if (newMethodParam)
            {
                // added both Express and standard - mail service as further options for the "method"
                dto.ShippingMethodParameter.AddShippingMethodParameterRow(
                    "Express", "Suspenders", newShippingMethod);

                dto.ShippingMethodParameter.AddShippingMethodParameterRow(
                    "Standard", "Galoshes", newShippingMethod);
            }

            // save it all
            bool saveItAll = false; // just for toggling here
            if (saveItAll)
            {
                ShippingManager.SaveShipping(dto);
            }
        }

        // ... old, not used, remove for others than RoCe, just looking
        public static void UpdateShippingMethods() // arg: "ship"
        {
            // get the rows
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false);

            // find the row
            ShippingMethodDto.ShippingMethodRow shippingMethodRow = null;
            foreach (ShippingMethodDto.ShippingMethodRow item in dto.ShippingMethod)
            {
                if (item.Name == "ExpressLetter")
                {
                    shippingMethodRow = item;
                }
            }

            // get the optionRow
            ShippingMethodDto.ShippingOptionRow optionRow = shippingMethodRow.ShippingOptionRow;

            bool willUpdate = false;

            if (willUpdate)
            {
                // added both Slow-and-Express-Letter
                dto.ShippingMethodParameter.AddShippingMethodParameterRow("SlowLetter", "Suspenders", shippingMethodRow);
                ShippingManager.SaveShipping(dto);
            }

            ShippingMethodDto.ShippingMethodParameterRow[] paramRows = shippingMethodRow.GetShippingMethodParameterRows();

            string paramValue = String.Empty;
            foreach (var item in paramRows)
            {
                if (item.Parameter == "ExpressLetter" || item.Parameter == "SlowLetter")
                {
                    // if this one is "Suspenders" ... we can use this ShippingMethod
                    paramValue = item.Value;
                }
            }
        }

        // remove for others than RoCe, just looking
        public static void LookForMethodCases() // arg: "case"
        {
            var dto = ShippingManager.GetShippingMethodsByMarket("sv", true);
            var m = dto.ShippingMethodCase;
        }


        #endregion // Shipping

        #region No demo-Not used in course
        // poking around some (tutti-frutti)
        public ActionResult TestLink()
        {
            //ContentReference theRef = new ContentReference(25);
            object obj = new object();


            //return RedirectToAction("Index", new { page = new ContentReference(25) }.page.ToPageReference()); // ok
            return RedirectToAction("Index", new { node = ContentReference.StartPage }); // ok
            //return RedirectToAction("DeadEnd", new { node = theRef, passed = "Hello" });
        }

        // no demo
        Injected<IPageRouteHelper> routeHelper;
        public void CheckStuff()
        {
            //ConfigurationManager
            var thepage = routeHelper.Service.Page;
            //routeHelper.Service.Content.


        }

        public void TestBF(string id)
        {
            //SetupBF bf = new SetupBF();
            //bf.CreateClubCard();

        }

        public IEnumerable<OrderValues> CheckOnFind()
        {
            // works here, not in cmd-app, have a separate dll for the models (Address/Order-values) 
            DateTime fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.Now.Day);
            DateTime toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);


            IClient client = Client.CreateFromConfig();
            return client.Search<OrderValues>()
               .For("Long-Sleeve-Shirt-White-Small_1")
               .InField("LineItemCodes")
               .Filter(x => x.orderDate.InRange(
                    fromDate,
                    toDate))
               .GetResult();
        }

        // not in use here - look in cmd-app
        public void UpdateShippingMethods2() // this was here before pasting in cmd-stuff 
        {
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false);

            IEnumerable<ShippingMethodDto.ShippingMethodRow> methods = dto.ShippingMethod;
            foreach (ShippingMethodDto.ShippingMethodRow item in methods)
            {
                //if ((bool)item["SpecialShip"])
                //{

                //}
                //dto.ShippingMethod.shippingo
                var i = item.ShippingOptionRow.GetShippingOptionParameterRows();

                foreach (var item2 in i)
                {

                }
            }

            var ii = ServiceLocator.Current.GetInstance<IInventoryService>();


        }

        public void CheckInCache()
        {
            //CatalogCache.ReadThrough();

            string str = String.Empty;
            str += "ecf-ctlg";
            str += "en";
            str += "catalognode-";
            str += "CatalogEntryInfo-";
            str += 5.ToString(); // 5 = EntryId

            string m = MasterCacheKeys.GetEntryKey(5);
            List<string> ss = new List<string>();
            ss.Add(m);

            List<string> masterKeys = new List<string>();
            string masterKey = String.Empty;
            masterKey += "AnyNodeKey";
            masterKeys.Add(masterKey);

            ContentReference r = _referenceConverter.GetContentLink("Shirts_1");
            NodeContentBase eb = _contentLoader.Get<NodeContentBase>(r);

            Func<EntryContentBase, EntryContentBase> stuff = Dummy;

            //var x = stuff(eb);

            //CatalogCache.ReadThrough<EntryContentBase>(str,ss, new TimeSpan(1,1,1), stuff(eb) )   ;

            ISynchronizedObjectInstanceCache theCache =
                ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>();

            //theCache.ReadThrough<EntryContentBase>(str, stuff(eb), new CacheEvictionPolicy(str));
            CatalogNodeResponseGroup responseGroup =
                new CatalogNodeResponseGroup(CatalogNodeResponseGroup.ResponseGroup.CatalogNodeInfo);

            int catalogId = 1;
            int parentNodeId = 1;

            CatalogNodes nodes = CatalogCache.ReadThrough(str, masterKeys, CatalogConfiguration.Instance.Cache.CatalogNodeTimeout,
                () =>
                {
                    var dto = GetCatalogNodesDto(catalogId, parentNodeId, responseGroup);
                    return dto.CatalogNode.Count > 0
                        ? LoadNodes(dto, null, false, responseGroup)
                        : new CatalogNodes { CatalogNode = new CatalogNode[0] };
                });


        }

        public static CatalogNodes LoadNodes(CatalogNodeDto dto, CatalogNode parent, bool recursive, CatalogNodeResponseGroup responseGroup)
        {
            List<CatalogNode> list = new List<CatalogNode>();
            foreach (CatalogNodeDto.CatalogNodeRow row in dto.CatalogNode)
            {
                CatalogNode item = LoadNode(row, recursive, responseGroup);
                item.ParentNode = parent;
                list.Add(item);
            }
            return new CatalogNodes { CatalogNode = list.ToArray() };
        }

        public static CatalogNode LoadNode(CatalogNodeDto.CatalogNodeRow row, bool recursive, CatalogNodeResponseGroup responseGroup)
        {
            CatalogNode parent = null;
            if (responseGroup == null)
            {
                throw new ArgumentNullException("responseGroup");
            }
            if (row != null)
            {
                CatalogNodeResponseGroup group;
                parent = new CatalogNode(row, responseGroup.ResponseGroups);
                if (!responseGroup.ContainsGroup(CatalogNodeResponseGroup.ResponseGroup.CatalogNodeFull) && !responseGroup.ContainsGroup(CatalogNodeResponseGroup.ResponseGroup.Children))
                {
                    return parent;
                }
                CatalogNodeDto dto = CatalogContext.Current.GetCatalogNodesDto(row.CatalogId, row.CatalogNodeId, responseGroup);
                if (recursive)
                {
                    group = responseGroup;
                }
                else
                {
                    group = new CatalogNodeResponseGroup(CatalogNodeResponseGroup.ResponseGroup.CatalogNodeInfo);
                }
                CatalogNodes nodes = LoadNodes(dto, parent, recursive, group);
                parent.Children = nodes;
            }
            return parent;
        }

        // not much
        private CatalogNodeDto GetCatalogNodesDto(int catalogId, int parentNodeId, CatalogNodeResponseGroup responseGroup)
        {
            return CatalogContext.Current.GetCatalogNodesDto(catalogId, parentNodeId, responseGroup);
        }

        // nothing
        public EntryContentBase Dummy(EntryContentBase r)
        {
            return null;
        }

        #endregion

        #region Arindam

        public void GetArindamsPrices()
        {
            // In 11 we have CustomIPriceOptimizer (\infrastructure\pricing)
            // ...had a guy at course named Arindam that had this scenario as a requirement
            // more info in trainer guid lines and the overridden "optimizer class"

            // Get both prices, of course, when using the R/W-service
            ContentReference theRef = _referenceConverter.GetContentLink("OddPriceSku_1");
            var detailPrices = _priceDetailService.List(theRef);

            var entry = _contentLoader.Get<EntryContentBase>(theRef);
            CatalogKey catK = new CatalogKey(entry.Code);

            // lowest only by default, but not with "custom optimizer" 
            // can change both prices to qty 1 and only the higher price is retrieved
            var readOnlyPrices = _priceService.GetCatalogEntryPrices(catK);

            // make it simple...
            PriceFilter ArindamFilter = new PriceFilter()
            {
                Currencies = new Currency[] { "SEK" },
                CustomerPricing = new CustomerPricing[]
                    {
                        new CustomerPricing(CustomerPricing.PriceType.AllCustomers,null),
                    },
            };
            List<CatalogKeyAndQuantity> theList = new List<CatalogKeyAndQuantity>();

            // the below doesn't help in the default behaviour
            CatalogKeyAndQuantity catKeyAndQtyExample1 =
                new CatalogKeyAndQuantity(catK, 1);
            theList.Add(catKeyAndQtyExample1);

            CatalogKeyAndQuantity catKeyAndQtyExample2 =
                new CatalogKeyAndQuantity(catK, 2);
            theList.Add(catKeyAndQtyExample2);

            var theMarket = _marketService.GetMarket("sv");

            // lowest only by default
            var other = _priceService.GetPrices(
                  theMarket.MarketId
                , DateTime.Now
                , theList
                , ArindamFilter);
            // with original price optimizer... 
            // R/O-loader takes a filter, but Qty is ignored for this 3:rd overload (by default)
            // doesn't get the Qty of 2, get 2 prices but it's only for qty 1 as it's the lowest

            // with custom optimizer it works as expected - 2 prices ... as the UI shows
        }

        #endregion

        #region MDP Direct

        void MetaFieldsAndMetaClasses()
        {
            /* MetaClass & MetaField also exist in BF, that's why we qualify or use aliases */
            Mediachase.MetaDataPlus.Configurator.MetaClass MyClass = Mediachase.MetaDataPlus.Configurator.MetaClass.Create
                (MetaDataContext.DefaultCurrent,
                "MyNewMetaClass",
                "MyNewMetaClass",
                "tbl_MyNewMetaClass",
                0,
                false,
                "This is the description");

            Mediachase.MetaDataPlus.Configurator.MetaField MyField = Mediachase.MetaDataPlus.Configurator.MetaField.Create
                (MetaDataContext.DefaultCurrent
                , "Medichase.Commerce.Orders"
                , "TheFieldName"
                , "TheFieldName"
                , "The Description"
                , Mediachase.MetaDataPlus.Configurator.MetaDataType.ShortString
                , 50
                , false
                , true
                , true
                , false);

            MyClass.AddField(MyField);

        }

        #endregion

        #region Cert

        private void CheckForCert()
        {

        }

        #endregion
    }
}
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
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentLoader _contentLoader;
        public readonly AssetUrlResolver _assetUrlResolver;
        public readonly UrlResolver _urlResolver;
        public readonly IOrderRepository _orderRepository;

        public AdminPageController(
              IPriceService priceService
            , IPriceDetailService priceDetailService
            , ICurrentMarket marketService
            , ReferenceConverter referenceConverter
            , IContentLoader contentLoader
            , AssetUrlResolver assetUrlResolver
            , UrlResolver urlResolver
            , IOrderRepository orderRepository)
        {
            _priceService = priceService;
            _priceDetailService = priceDetailService;
            _currentMarket = marketService;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _assetUrlResolver = assetUrlResolver;
            _urlResolver = urlResolver;
            _orderRepository = orderRepository;
        }

        Injected<ICatalogSystem> _cat;

        public ActionResult Index(AdminPage currentPage)
        {

            string code = "Long-Sleeve-Shirt-White-Small_1";
            PricingService pricingService = new PricingService(_priceService, _currentMarket, _priceDetailService);

            IEnumerable<IPriceValue> thePriceList = pricingService.GetPrices(code);

            string group = string.Empty;
            string sortOrder = string.Empty;

            var model = new AdminPageViewModel
            {
                price = thePriceList.FirstOrDefault().UnitPrice.Amount.ToString("C"),
                prices = thePriceList,
            };

            return View(model);
        }

        Injected<IMarketService> _marketService;

        #region CheckingStuff_ECF_12
        public void CheckingStuff_ECF_12()
        {
            var curr = SiteContext.Current.Currency;

            // deprecated
            //SiteContext.Current.SiteId 

            var lang = SiteContext.Current.LanguageName;

            var a= Mediachase.Commerce.Core.AppContext.Current.AzureCompatible;
            var v = Mediachase.Commerce.Core.AppContext.GetProductVersion();
            var app = Mediachase.Commerce.Core.AppContext.Current.GetApplicationDto();

        }


        #endregion
        
        #region Markets

        public void CreateMarket()
        {
            IMarketService marketService =
                ServiceLocator.Current.GetInstance<IMarketService>();

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

            marketService.CreateMarket(newMarket);

        }

        // just an example
        public void DeleteMarket() // arg "dm"
        {
            IMarketService marketService =
                ServiceLocator.Current.GetInstance<IMarketService>();

            marketService.DeleteMarket("Spain");
        }

        #endregion

        #region Warehouses
        Injected<IWarehouseRepository> warehouseRepository;
        Injected<IInventoryService> inventoryService;
        public void CreateWarehouses()
        {
            
            List<string> warehouseNames = new List<string> { "Stockholm", "Nashua", "London" };
            
            foreach (var item in warehouseNames)
            {
                var warehouse = new Warehouse()
                {
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

                warehouseRepository.Service.Save(warehouse);

            } 
        }

        public void EditWarehouse()
        {
            var warehouse = warehouseRepository.Service.Get("test");
            //var writableCloneWarehouse = new Warehouse(warehouse);
            //writableCloneWarehouse.IsPickupLocation = true;
            //warehouseRepository.Service.Save(writableCloneWarehouse);

            var ir = inventoryService.Service.Get("dummy_1", warehouse.Code);
            var ir2 = ir.CreateWritableClone();
            ir2.AdditionalQuantity = 5; // "reserved"

            var list = new List<InventoryRecord>
            {
                ir2
            };
            inventoryService.Service.Save(list);
        }

        public void CheckInventory()
        {
            string entryCode = "dummy_1";
            var warehouse = warehouseRepository.Service.Get("Test");
            var inventoryRecord = inventoryService.Service.Get(entryCode, warehouse.Code);

            decimal available = inventoryRecord.PurchaseAvailableQuantity;
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
            // need a category (Catalog)
            CatalogTaxDto t_Dto = CatalogTaxManager.CreateTaxCategory("VAT", true);

            // need a JurisdictionDto ... works like a bucket
            // Have some code in pptx ... 
            JurisdictionDto jurisdictionDto = JurisdictionManager.GetJurisdictions
                (JurisdictionManager.JurisdictionType.Tax);
            JurisdictionDto.JurisdictionRow jurisdictionRow =
                jurisdictionDto.Jurisdiction.NewJurisdictionRow();
            jurisdictionRow.County = "HomeLand";
            jurisdictionRow.DisplayName = "HomeLand";
            jurisdictionRow.District = "WholeCountry";
            jurisdictionRow.CountryCode = "se";
            jurisdictionRow.Code = "se";

            jurisdictionRow.JurisdictionType =
                (int)JurisdictionManager.JurisdictionType.Tax; // found in CM

            // ...easy to forget
            jurisdictionDto.Jurisdiction.AddJurisdictionRow(jurisdictionRow);

            // Groups, another bucket ... 
            JurisdictionDto.JurisdictionGroupRow jurisdictionGroup =
                jurisdictionDto.JurisdictionGroup.NewJurisdictionGroupRow();
            jurisdictionGroup.DisplayName = "HomeLand Group";
            jurisdictionGroup.Code = "se_gr";
            jurisdictionGroup.JurisdictionType =
                JurisdictionManager.JurisdictionType.Tax.GetHashCode(); // found in CM
            jurisdictionDto.JurisdictionGroup.AddJurisdictionGroupRow(jurisdictionGroup);

            JurisdictionDto.JurisdictionRelationRow jurisdictionRelation =
                jurisdictionDto.JurisdictionRelation.NewJurisdictionRelationRow();
            jurisdictionRelation.JurisdictionRow = jurisdictionRow;
            jurisdictionRelation.JurisdictionGroupRow = jurisdictionGroup;
            jurisdictionDto.JurisdictionRelation.AddJurisdictionRelationRow(jurisdictionRelation);

            JurisdictionManager.SaveJurisdiction(jurisdictionDto);
        }

        public void CreateTaxes()
        {
            
            TaxType taxType = TaxType.SalesTax;

            TaxDto orderTaxDto = TaxManager.GetTaxDto(taxType); // cheating with the language - correct later

            TaxDto.TaxRow taxRow = orderTaxDto.Tax.AddTaxRow(
                 taxType.GetHashCode()
                , "HomeLand_VAT"
                , 10 // SortOrder
                );

            int taxId = taxRow.TaxId;
            
            TaxDto.TaxValueRow taxValueRow = orderTaxDto.TaxValue.NewTaxValueRow();

            taxValueRow.TaxId = taxId;
            taxValueRow.JurisdictionGroupId =
                JurisdictionManager.GetJurisdictionGroup("se_gr") // is the code
                .JurisdictionGroup[0].JurisdictionGroupId;

            CatalogTaxDto taxDto = CatalogTaxManager.GetTaxCategories();
            int taxInt = 0;
            foreach (var item in taxDto.TaxCategory)
            {
                if (item.Name == "VAT")
                {
                    taxInt = item.TaxCategoryId;
                }
            }
            // just to show
            taxValueRow.TaxCategory = CatalogTaxManager.GetTaxCategoryNameById(taxInt);

            taxValueRow.Percentage = double.Parse("25");
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
            FilterElementCollection fc = new FilterElementCollection
            {
                FilterElement.EqualElement("FirstName", "admin")
            };

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


        #region BF Permissions in 9+ - undocumented stuff
        //  (introduced late 8... I think)

        private const string Commerce = "EPiCommerce";
        private static readonly PermissionRepository PermissionRepository =
            ServiceLocator.Current.GetInstance<PermissionRepository>();

        #endregion
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

        // Another not documented "feature" that could come in handy
        public void GetPoData() // Fetch orders by specific LineItems
        {
            // RoCe: fix this - it's not complete
            // Not the official way, and kind of hacky ... but it works :)
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

            // note: ...shouldn't do like this... use .ctor
            var associationDefinitionRepository =
                ServiceLocator.Current.GetInstance<GroupDefinitionRepository
                <AssociationGroupDefinition>>();

            // if those already exist, nothing happens (! ...it gets into DDS !)
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
        }

        Injected<IAssociationRepository> _assocRep;
        public void AddAssociation() // the CMS-way
        {
            ContentReference sourceLink = _referenceConverter.GetContentLink("Long-Sleeve-Shirt-White-Small_1");
            ContentReference targetLink = _referenceConverter.GetContentLink("Long-Sleeve-Shirt-Blue-Medium_1");

            EPiServer.Commerce.Catalog.Linking.Association association =
                new EPiServer.Commerce.Catalog.Linking.Association();

            association.Source = sourceLink;
            association.Target = targetLink;
            association.SortOrder = -10;
            association.Type = new AssociationType() { Id = "Cool", Description = "NiceToHave" };
            //association.Type = new AssociationType() { Id = "Buttons", Description = "For suspenders" };
            //association.Type = new AssociationType() { Id = "Clips", Description = "For suspenders" };
            //association.Group = new AssociationGroup() { Name = "Accessories", Description = "Spice up a customer", SortOrder = 0 };
            association.Group = new AssociationGroup() { Name = "CrossSell", Description = "Give a customer more choices", SortOrder = 0 };
            //association.Group = new AssociationGroup() { Name = "UpSell", Description = "Give a happier customer", SortOrder = 0 };

            // in 10 
            //_linksRepository.UpdateAssociation(association); // Note: there is no "Save..."

            // in 11
            _assocRep.Service.UpdateAssociation(association);

        }

        // Old-school, New-school
        public void AddAssociationForTest()
        {
            ContentReference sourceLink =
                _referenceConverter.GetContentLink("Trousers-with-buttons_1");
            ContentReference targetLink =
                _referenceConverter.GetContentLink("Galoshes_1");

            // Do it new school, same as above
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
                    }, // NO "Sort"

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

            // Do it old-school using DTOs
            CatalogAssociationDto dto = CatalogContext.Current.GetCatalogAssociationDto(-1); // just to get a bucket -1 means empty

            CatalogAssociationDto.AssociationTypeRow tRow = dto.AssociationType.NewAssociationTypeRow();
            tRow.AssociationTypeId = "BadBusiness";
            tRow.Description = "Have to change manager";

            dto.AssociationType.AddAssociationTypeRow(tRow);
            // This is what we have

            // ...need to do this before saving "the rest"
            CatalogContext.Current.SaveAssociationType(dto); // This is kind of akward, does not follow the pattern
            // ...found the below comment in the Mediachase.Commerce.Catalog.Data.CatalogAssociationAdmin
            // "Saves changes to the current DTO's associations. Changes to AssociationType are not saved."

            CatalogAssociationDto.CatalogAssociationRow aRow =
                dto.CatalogAssociation.NewCatalogAssociationRow();
            aRow.AssociationDescription = "This is the Association Description";
            aRow.AssociationName = "HappySelling";
            aRow.CatalogEntryId = _referenceConverter.GetObjectId(targetLink);
            aRow.SortOrder = 100; // Mandatory here in code
            // have to do this...
            dto.CatalogAssociation.AddCatalogAssociationRow(aRow);

            CatalogAssociationDto.CatalogEntryAssociationRow eRow =
                dto.CatalogEntryAssociation.NewCatalogEntryAssociationRow();
            eRow.AssociationTypeId = tRow.AssociationTypeId;
            eRow.CatalogEntryId = _referenceConverter.GetObjectId(sourceLink);
            eRow.SortOrder = 99; // mandatory
            eRow.CatalogAssociationId = aRow.CatalogAssociationId;
            dto.CatalogEntryAssociation.AddCatalogEntryAssociationRow(eRow);

            // and now it's "Save"
            CatalogContext.Current.SaveCatalogAssociation(dto); // Note: look above... at the saving of "A-type"... else
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

            CatalogAssociationDto dto = CatalogContext.Current.GetCatalogAssociationDto("Accessories");
            IEnumerable<CatalogAssociationDto.AssociationTypeRow> tRows = dto.AssociationType;
            IEnumerable<CatalogAssociationDto.CatalogAssociationRow> aRows = dto.CatalogAssociation;
            IEnumerable<CatalogAssociationDto.CatalogEntryAssociationRow> eRows = dto.CatalogEntryAssociation;
        }

        #endregion


        #region Shipping 

        public void CheckForShippingOptions() // 
        {
            IMarket market = _marketService.Service.GetMarket(new MarketId("sv"));

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
               ...and there is...as a case for an exerciseand demo
               You could of course do/create this lookup in many custom ways, 
               but the goal is to use something already in place in Commerce */

            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket
                (MarketId.Default.Value, false); // ...just using the "Default Market" as an example
            // Have the dbo.MarketShippingMethods table if we want to investigate further

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
        
        #endregion // Shipping

      
        #region Arindam

        public void GetFunnyPrices()
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
            PriceFilter FunnyFilter = new PriceFilter()
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

            var theMarket = _marketService.Service.GetMarket("sv");

            // lowest only by default
            var other = _priceService.GetPrices(
                  theMarket.MarketId
                , DateTime.Now
                , theList
                , FunnyFilter);
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
    }
}
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Infrastructure.Pricing
{
    // This one is from "fund" ... not maintaned here... lots of squigglies
    public class PricingService
    {

        // added
        Injected<IContentLoader> contentLoader;
        Injected<ReferenceConverter> referenceConverter;
        Injected<ICatalogSystem> catalogSystem;
        Injected<ICurrentMarket> currentMarket;

        // R/O
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _marketService;
        // R/W
        private readonly IPriceDetailService _priceDetailService;
        public PricingService(IPriceService priceService
            , ICurrentMarket marketService
            , IPriceDetailService priceDetailService)
        {
            _priceService = priceService;
            _marketService = marketService;
            _priceDetailService = priceDetailService;
        }


        private void CheckPricingLoaders(ContentReference contentReference, CustomerPricing customerPricing)
        {

            PricingLoader RwLoader = new PricingLoader(contentLoader.Service, _priceDetailService, referenceConverter.Service, catalogSystem.Service);
            ItemCollection<PriceDetail> prices = RwLoader.GetPrices(contentReference, MarketId.Default, customerPricing);

            // Overload is marked "Obsolete" - "Use the constructor with currentMarketService instead."
            ReadOnlyPricingLoader RoLoader = new ReadOnlyPricingLoader( //  first overload obsoleted in 9
                contentLoader.Service
                , _priceService
                , referenceConverter.Service
                , catalogSystem.Service
                , currentMarket.Service // added
                                        //, Mediachase.Commerce.Security.SecurityContext.Current // added
                , CustomerContext.Current // added
                                          //, FrameworkContext.Current); // added
                );

        }

        // Demo - Fund
        public void CheckPrices(EntryContentBase CurrentContent)
        {
            // Pricing, a long story with many alternative

            /*
             * Two services are available for interacting with price data. 
             *  - The IPriceService API is typically used by order processing to fetch prices for actual use
             *  - IPriceDetailService is typically used in "integration" and interface/code for display (all prices) and edit prices. 
             
             The difference between these APIs is that the IPriceService works with optimized sets of price data, 
             * while the IPriceDetailService works with "the truth". 
             * The optimization in the IPriceService 
             * removes prices that will cannot be used, and trims or splits prices that overlap. 
             */

            #region StoreHelper...does this

            /*Steps in StoreHelper
             *
              - Add AllCustomers
             * Check Cust-Group
             * ...Effective-Cust-Group
             * Get the service
             * Set the filter
             * Fetch ...  priceService.GetPrices + get the cheapest (note: always cheapest)
             * Get a ... IPriceValue and then...
             * ....return new Price(priceValue.UnitPrice);
             *  
             * public static Price GetSalePrice(Entry entry, decimal quantity, IMarket market, Currency currency)
             * 
             */
            #endregion

            // ... look in VariantController for nice extensions
            
            #region GetDefaultPrice...does this

            /*
             Could have a look at .GetDefaultPrice in Reflector
             PricingExtensions (EPiServer.Commerce.Catalog.ContentTypes)
              - GetDefaultPrice() // gets the R/O-loader

              - tries to figure out  the Currency
              - tries to figure out Market, DateTime, CatalogKey... and does...
              - IPriceValue GetDefaultPrice(MarketId market, DateTime validOn, CatalogKey catalogKey, Currency currency);
               ...goes to the concrete impl. för the R/O-provider and creates a PriceFilter 
               ....and gets a "default price" for a catalog entry. The "default price" for a 
                    ....market, currency, and catalog entry is the price available to 
                    "all customers" at a minimum quantity of 0.
                ...that in turn goes to the Provider and say GetPrices() ... with all the stuff found
             */

            #endregion


            // PriceDetails ... administrative stuff, don´t use for retrieving prices for the store
            // 3 overloads... the 3:rd with filter & Market
            List<IPriceDetailValue> priceList1 = _priceDetailService.List(CurrentContent.ContentLink).ToList();
            EPiServer.Commerce.SpecializedProperties.Price aPrice =
                new EPiServer.Commerce.SpecializedProperties.Price(priceList1.FirstOrDefault()); // just checking
            // ...have a CreatePrice("theCode"); // further below


            /* dbo.PriceType.sql
                0	All Customers
                1	Customer
                2	Customer Price Group
            */
            
            // DB-side paging, GetPrices() takes start-count, is R/O ... it´s a Loader
            PricingLoader detailedPricingLoader = ServiceLocator.Current.GetInstance<PricingLoader>();
            // detailedPricingLoader.GetPrices()

            // Good set of methods, no paging (GetChildrenPrices-obsoleted), gets it all
            ReadOnlyPricingLoader readOnlyPricingLoader = ServiceLocator.Current.GetInstance<ReadOnlyPricingLoader>();



            // detailedPricingLoader. as the service
            var p = readOnlyPricingLoader.GetDefaultPrice(CurrentContent.ContentLink);
            // use this "loader" on the front-end...instead of the service

            // PriceService R/O-pricing ("the optimized service" (from the good old R3-era ´:`))
            // ... that´s why we have some confusing stuff left in the system (it entered here before the "Content-Model" was in place)
            CatalogKey catKey = new CatalogKey(CurrentContent.Code); // Catalogkey... example of legacy

            // return full sets of price data for the specified catalog entries?
            // ...also takes an Enumerable 
            var pricesByKey = _priceService.GetCatalogEntryPrices(catKey); // need catalogKey ( or IEnumerable of those )
            var priceByDefault = _priceService.GetDefaultPrice(// Market, time, CatKey & Currency
                currentMarket.Service.GetCurrentMarket().MarketId // ...better than MarketId.Default
                , DateTime.UtcNow
                , catKey
                , new Currency("usd"));

            CatalogKeyAndQuantity keyAndQtyExample = new CatalogKeyAndQuantity(catKey, 12);
            // first overload as GetDefaultPrice
            // second as an IEnumerable of keys
            // third as an IEnumerable of keysAndQty


            // If custom built "Optimized"...
            // ...then need a mechanism for synchronizing with a custom detail service, then it must call 
            // IPriceDetailService.ReplicatePriceServiceChanges on all edits to update the optimized data store
            
            /* Price Filter */

            List<Currency> currencies = new List<Currency>();
            List<CustomerPricing> custprices = new List<CustomerPricing>();

            // SaleCode (UI) or PriceGroup (code) (string) ... the Cust-Group
            // CM / CMS UI: SaleType - SaleCode
            // API: PriceType, PriceCode (string)

            PriceFilter filter = new PriceFilter()
            {
                Quantity = 2M,
                Currencies = new Currency[] { "USD", "SEK" },
                CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing(CustomerPricing.PriceType.AllCustomers,null),
                    new CustomerPricing(CustomerPricing.PriceType.PriceGroup, "MyBuddies") // or several...
                },
                ReturnCustomerPricing = true // ...see below for info
                // interpretation of the CustomerPricing property... if true; gets all that applies
            };

            #region Info ReturnCustomerPricing

            /* The ReturnCustomerPricing property controls the interpretation of the CustomerPricing property. 
             * If the value of this property is false, and multiple price values that are identical except for the customer pricing 
             * (but both match the prices targeted by the method call) could be returned, then only the entry in that grouping with 
             * the lowest price will be returned (this is the more common use case). If the value of this property is true, 
             * then all prices will be returned individually. The default value is false. As an example, 
             *  suppose a catalog entry has a price of $10.00 for all customers, and $9.00 for one particular customer. 
             *  A call to a GetPrices method that would match both prices, and has ReturnCustomerPricing set to false, 
             *  would only get the $9.00 price in the result set. If ReturnCustomerPricing was set to true for the same call, 
             *  both the $9.00 and $10.00 price would be returned. */

            #endregion

            // The rest needed, CatKey, Market, TimeStamp
            CatalogKey catalogKey = new CatalogKey(CurrentContent.Code); // 2 useful overloads

            IEnumerable<IPriceValue> priceValues = _priceService.GetPrices(
                MarketId.Default, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);

            decimal onePrice = priceValues.FirstOrDefault().UnitPrice.Amount; // just checking
        }

        public decimal GetTheRightPrice(EntryContentBase CurrentContent)
        {
            IEnumerable<IPriceValue> priceValues;
            // need to check if anonymous or not ... so the EffectiveGroup does not bother 
            // Anonymous... could use the DefaultPrice, but may miss the tiered price if set at Anonymous
            if (CustomerContext.Current.CurrentContact == null)
            {
                PriceFilter filter = new PriceFilter()
                {
                    Quantity = 1M,
                    Currencies = new Currency[] { _marketService.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                    CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing(CustomerPricing.PriceType.AllCustomers, null),
                },
                    ReturnCustomerPricing = false // 
                };

                // The rest needed, CatKey, Market, TimeStamp
                CatalogKey catalogKey = new CatalogKey(CurrentContent.Code); // 3 overloads

                priceValues = _priceService.GetPrices(
                    currentMarket.Service.GetCurrentMarket().MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);
            }
            else
            {
                // Logged on
                // no custom PriceTypes... yet
                PriceFilter filter = new PriceFilter()
                {
                    Quantity = 1M,
                    Currencies = new Currency[] { _marketService.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                    CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing(CustomerPricing.PriceType.AllCustomers, null),
                    new CustomerPricing(CustomerPricing.PriceType.PriceGroup, CustomerContext.Current.CurrentContact.EffectiveCustomerGroup), // or several...
                    new CustomerPricing(CustomerPricing.PriceType.UserName, CustomerContext.Current.CurrentContact.FirstName)
                },
                    ReturnCustomerPricing = false // 
                    // ... if true; gets all that applies
                };

                // The rest needed, CatKey, Market, TimeStamp
                CatalogKey catalogKey = new CatalogKey(CurrentContent.Code); // 

                priceValues = _priceService.GetPrices(
                   currentMarket.Service.GetCurrentMarket().MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);
            }

            if (priceValues.Count() > 0)
            {
                return priceValues.ToList().OrderBy(x => x.UnitPrice.Amount).FirstOrDefault().UnitPrice.Amount;
            }
            else
            {
                return 0; // should not actually, could use default price... it's a demo 
            }
        }

        // Newer ECF 11 - Oct-17
        internal Price GetTheRightCustomerPrice(ShirtVariation currentContent)
        {
            IEnumerable<IPriceValue> priceValues;
            // need to check if anonymous or not ... so the EffectiveGroup does not bother 
            // Anonymous... could use the DefaultPrice, but may miss the tiered price if set at Anonymous
            if (CustomerContext.Current.CurrentContact == null)
            {
                PriceFilter filter = new PriceFilter()
                {
                    Quantity = 1M,
                    Currencies = new Currency[] { _marketService.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                    CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing(CustomerPricing.PriceType.AllCustomers, null),
                },
                    ReturnCustomerPricing = false // 
                };

                // The rest needed, CatKey, Market, TimeStamp
                CatalogKey catalogKey = new CatalogKey(currentContent.Code); 

                priceValues = _priceService.GetPrices(
                    currentMarket.Service.GetCurrentMarket().MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);
            }
            else
            {
                // Logged on
                // no custom PriceTypes... yet
                PriceFilter filter = new PriceFilter()
                {
                    Quantity = 1M,
                    Currencies = new Currency[] { _marketService.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                    CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing(CustomerPricing.PriceType.AllCustomers, null),
                    new CustomerPricing(CustomerPricing.PriceType.PriceGroup, CustomerContext.Current.CurrentContact.EffectiveCustomerGroup), // or several...
                    new CustomerPricing(CustomerPricing.PriceType.UserName, CustomerContext.Current.CurrentContact.FirstName)
                },
                    ReturnCustomerPricing = false // 
                    // ... if true; gets all that applies
                };

                // The rest needed, CatKey, Market, TimeStamp
                CatalogKey catalogKey = new CatalogKey(currentContent.Code); // 3 overloads

                priceValues = _priceService.GetPrices(
                   currentMarket.Service.GetCurrentMarket().MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);
            }

            if (priceValues.Count() > 0)
            {
                return new Price(priceValues.ToList().OrderBy(x => x.UnitPrice.Amount).FirstOrDefault());
            }
            else
            {
                return new Price(); // should not actually, could use default price... it's a demo 
            }

        }

        public IEnumerable<IPriceValue> GetPrices(string code)
        {
            List<IPriceValue> priceList = new List<IPriceValue>();
            IMarket market = _marketService.GetCurrentMarket(); // DEFAULT

            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("Code is needed");
            }

            // need the key 
            var catalogKey = new CatalogKey(code);

            priceList = _priceService.GetCatalogEntryPrices(catalogKey).ToList();

            foreach (IPriceValue item in priceList)
            {
                if (item.CustomerPricing.PriceTypeId.ToString() == "7") // could use the int
                {
                    priceList.Clear();
                    break;
                }
            }

            // custom PriceTypeId, a bit cumbersome
            PriceFilter filter = new PriceFilter()
            {
                Quantity = 0M,
                Currencies = new Currency[] { "USD" },
                CustomerPricing = new CustomerPricing[]
                {
                    new CustomerPricing((CustomerPricing.PriceType)7,"VIP") // or "Andersson"... need the code...or
                    // ... do filtering (like remove from list)
                },
                ReturnCustomerPricing = true // ...see below for info
                // interpretation of the CustomerPricing property... if true; gets all that applies
            };

            priceList = _priceService.GetPrices(
                _marketService.GetCurrentMarket().MarketId.Value, DateTime.Now, catalogKey, filter).ToList();

            // just checking
            var p = new PriceValue();
            p.UnitPrice = new Money(99, "USD");
            p.MinQuantity = 2;
            priceList.Add((IPriceValue)p);

            return priceList;
        }

        public IEnumerable<IPriceValue> FakePromotion(EntryContentBase sku) // support case
        {
            //List<IPriceValue> priceList = new List<IPriceValue>();
            IMarket market = _marketService.GetCurrentMarket();

            // ...first add the default for all customer pricing 
            List<CustomerPricing> customerPricing = new List<CustomerPricing>
            {
                new CustomerPricing(CustomerPricing.PriceType.AllCustomers, string.Empty)
            };

            // ...then add the price type specific to a user and to a possible price group            
            IPrincipal currentUser = PrincipalInfo.CurrentPrincipal;

            if (CustomerContext.Current.CurrentContact != null)
            {
                if (!string.IsNullOrEmpty(currentUser.Identity.Name))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.UserName, currentUser.Identity.Name));
                }

                CustomerContact currentUserContact = CustomerContext.Current.CurrentContact;

                if (currentUserContact != null && !string.IsNullOrEmpty(currentUserContact.EffectiveCustomerGroup))
                { // Could look for both Org & Cust.
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup,
                        currentUserContact.EffectiveCustomerGroup));
                }
            }

            // Add the BasePrice (in this case the "PromotionPrice")
            customerPricing.Add(new CustomerPricing((CustomerPricing.PriceType)3, string.Empty));

            PriceFilter filter = new PriceFilter()
            {
                Quantity = 1M,
                Currencies = new Currency[] { "USD" },
                CustomerPricing = customerPricing,
                ReturnCustomerPricing = true // note this arg.
            };

            CatalogKey catalogKey = new CatalogKey(sku.Code);

            return _priceService.GetPrices(
                market.MarketId.Value, DateTime.Now, catalogKey, filter).ToList();
        }

        public void CreatePrice(string code)
        {
            List<IPriceDetailValue> newPrices = new List<IPriceDetailValue>();

            var priceDetailValue = new PriceDetailValue
            {
                CatalogKey = new CatalogKey(code),
                MarketId = new MarketId("DEFAULT"),
                CustomerPricing = CustomerPricing.AllCustomers,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidUntil = DateTime.UtcNow.AddYears(1),
                MinQuantity = 5m,
                UnitPrice = new Money(95m, Currency.USD)
            };

            newPrices.Add(priceDetailValue);

            _priceDetailService.Save(newPrices);

        }


    }
}
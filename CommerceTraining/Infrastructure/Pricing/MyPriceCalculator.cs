using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace CommerceTraining.Infrastructure.Pricing
{
    [ServiceConfiguration(typeof(MyPriceCalculator),
    Lifecycle = ServiceInstanceScope.Singleton)]
    public class MyPriceCalculator
    {
        ICurrentMarket _currentMarket;
        IPriceService _priceService;

        public MyPriceCalculator
            (
                ICurrentMarket currentMarket,
                IPriceService priceService
            )
        {
            _currentMarket = currentMarket;
            _priceService = priceService;
        }

        private List<CustomerPricing> GetCustomerPricingList() // 
        {
            // Add the standard stuff, not "BasePrice" 
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
                    customerPricing.Add(new CustomerPricing(
                        CustomerPricing.PriceType.UserName,
                        currentUser.Identity.Name));
                }

                CustomerContact currentUserContact = CustomerContext.Current.CurrentContact;

                if (currentUserContact != null && !string.IsNullOrEmpty(currentUserContact.EffectiveCustomerGroup))
                {
                    customerPricing.Add(new CustomerPricing(
                        CustomerPricing.PriceType.PriceGroup,
                        currentUserContact.EffectiveCustomerGroup));
                }
            }

            return customerPricing;
        }

        // Where it happens for the new Promos/Cust-Pricing...need to clean this mess upp
        public Price CheckDiscountPrice(
            EntryContentBase entry,
            decimal quantity,
            decimal promoPrice)
        {
            // Get the list
            var customerPricing = GetCustomerPricingList();
            // Add BasePrice - Lowest possible
            customerPricing.Add(new CustomerPricing((CustomerPricing.PriceType)3, string.Empty));

            // Define the PriceFilter
            PriceFilter filter = new PriceFilter()
            {
                Quantity = quantity, // need improvements here
                Currencies = new Currency[] { _currentMarket.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                CustomerPricing = customerPricing, // changed
                ReturnCustomerPricing = true // 
                // ... if true; gets all that applies
            };
            
            // The rest needed, CatKey, Market, TimeStamp
            CatalogKey catalogKey = new CatalogKey(entry.Code); // 3 overloads

            #region This is old stuff, may not use

            /* more hassle to get it working you could say, but used in the right way ... 
             * ...it could simplyfy ... but it's still old */
            //ItemCollection<Price> prices = pricingSKU.GetPrices(
            //    _readOnlyPricingLoader.Service
            //    , _currentMarket.Service.GetCurrentMarket().MarketId
            //    , customerPricing);

            #endregion

            // ToDo: Get all applicable prices
            //IEnumerable<IPriceValue> prices = null; // starter

            IEnumerable<IPriceValue> prices = // Solution
                _priceService.GetPrices(_currentMarket.GetCurrentMarket().MarketId
                , DateTime.Now, catalogKey
                , filter);

            #region Old garbage


            //ToDo: Identify the lowest price when the "base-price is excluded"
            // Outcommented is starter-code to make things work ...
            // ...exchange the below for lab-code
            //Price p = new Price();
            //IPriceValue lowPrice = p.ToPriceValue();

            // Solution
            //IPriceValue lowPrice = prices.Where(x => x.MinQuantity <= quantity
            //    && x.CustomerPricing.PriceTypeId != (CustomerPricing.PriceType)3) // do not look on "BasePrice"
            //    .OrderBy(pv => pv.UnitPrice).FirstOrDefault();

            #endregion

            //ToDO: Get the base price (which is the lowest possible price)
            //IPriceValue basePrice = null; // is the starter

            #region New garbage

            //_priceService.GetPrices()
            //IPriceValue basePrice2 = null;
            //if (prices.Where(p => p.CustomerPricing.PriceTypeId == (CustomerPricing.PriceType)3).Any())
            //{

            #endregion

            // whatever price comes out
            IPriceValue lowestPrice = prices.Where
                (p => p.CustomerPricing.PriceTypeId != (CustomerPricing.PriceType)3).First();
            
            // get the base price
            IPriceValue basePrice = prices.Where(
                x => x.CustomerPricing.PriceTypeId == (CustomerPricing.PriceType)3).First();
            
            // Solution, pick the base price if promos goes below
            if (basePrice != null && basePrice.UnitPrice.Amount >= promoPrice) // new promos
            {
                return new Price(basePrice);
            }
            else
            {
                // returning the promo-Price ... comes as an arg. (decimal)
                return new Price
                {
                    UnitPrice = new Money(promoPrice, _currentMarket.GetCurrentMarket().DefaultCurrency),
                    ValidFrom = lowestPrice.ValidFrom,
                    ValidUntil = lowestPrice.ValidUntil,
                    MinQuantity = lowestPrice.MinQuantity,
                    MarketId = lowestPrice.MarketId,
                    EntryContent = entry,
                    CustomerPricing = lowestPrice.CustomerPricing
                };
            }
        }

        // new method for the...
        // \Infrastructure\Promotions\CustomPromotionEngineContentLoader.cs
        public IPriceValue GetSalePrice(EntryContentBase entry, decimal quantity)
        {
            // some basic validation
            if (entry == null)
                throw new NullReferenceException("entry object can't be null");

            if (entry as IPricing == null)
                throw new InvalidCastException("entry object must implement IPricing");

            // Need the pricing context... 
            // Get the groups
            List<CustomerPricing> customerPricing = GetCustomerPricingList();

            IMarket theMarket = _currentMarket.GetCurrentMarket();

            IEnumerable<Currency> currencies = theMarket.Currencies;

            PriceFilter filter = new PriceFilter()
            {
                Quantity = quantity,
                Currencies = currencies,
                CustomerPricing = customerPricing,
                ReturnCustomerPricing = false
            };

            CatalogKey catalogKey = new CatalogKey(entry.Code); // 3 overloads

            //_pricingLoader.Service.GetPrices(entryLink,theMarket.MarketId.Value)
            IEnumerable<IPriceValue> prices = _priceService.GetPrices(
                theMarket.MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);

            // doing for promos
            if (prices.Count() >= 1)
            {
                return prices.OrderBy(p => p.UnitPrice.Amount).First(); //...
            }
            else
            {
                return new PriceValue();
            }
            
        }


    }
}
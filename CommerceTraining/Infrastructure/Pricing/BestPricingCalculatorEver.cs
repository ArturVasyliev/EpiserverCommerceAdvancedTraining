using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Objects;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Price = EPiServer.Commerce.SpecializedProperties.Price;
using EPiServer.Core;
using EPiServer;

namespace CommerceTraining.Infrastructure.Pricing
{
    public class BestPricingCalculatorEver
    {

        private static Injected<ICurrentMarket> _currentMarket { get; set; }
        // May not use this one for the below actions. it's "older"
        //private static Injected<ReadOnlyPricingLoader> _readOnlyPricingLoader { get; set; } 
        private static Injected<PricingLoader> _pricingLoader { get; set; }
        private static Injected<IPriceService> _priceService { get; set; }
        //private static Injected<ReadOnlyPricingLoader> _roPriceLoader;
        private static Injected<IContentLoader> _contentLoader;

        // Where it happens for the new Promos/Cust-Pricing...need to clean this mess upp
        public static Price GetDiscountPrice(
            EntryContentBase contentReference, int quantity, decimal promoPrice /*, string catalogName, string categoryName*/)
        {
            // some basic validation
            if (contentReference == null)
                throw new NullReferenceException("entry object can't be null");

            if (contentReference as IPricing == null)
                throw new InvalidCastException("entry object must implement IPricing");

            // Define the PriceFilter
            PriceFilter filter = new PriceFilter()
            {
                Quantity = 0M, // need improvements here
                Currencies = new Currency[] { _currentMarket.Service.GetCurrentMarket().Currencies.FirstOrDefault() }, // only have one at the moment...
                CustomerPricing = GetCustomerPricingList(), // changed
                ReturnCustomerPricing = true // 
                // ... if true; gets all that applies
            };

            // The rest needed, CatKey, Market, TimeStamp
            CatalogKey catalogKey = new CatalogKey(contentReference.Code); // 3 overloads

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
                _priceService.Service.GetPrices(_currentMarket.Service.GetCurrentMarket().MarketId
                , DateTime.Now, catalogKey
                , filter);

            //ToDo: Identify the lowest price when the "base-price is excluded"
            // Outcommented is starter-code to make things work ...
            // ...exchange the below for lab-code
            //Price p = new Price();
            //IPriceValue lowPrice = p.ToPriceValue();

            // Solution
            IPriceValue lowPrice = prices.Where(x => x.MinQuantity <= quantity
                && x.CustomerPricing.PriceTypeId != (CustomerPricing.PriceType)3) // do not look on "BasePrice"
                .OrderBy(pv => pv.UnitPrice).FirstOrDefault();

            //ToDO: Get the base price (which is the lowest possible price)
            //IPriceValue basePrice = null; // is the starter

            // use as solution 
            IPriceValue basePrice = prices.Where(
                x => x.CustomerPricing.PriceTypeId == (CustomerPricing.PriceType)3).First();

            // ...should check the RG... and reload if not filled with nodes
            Entry entry =
                contentReference.LoadEntry(CatalogEntryResponseGroup.ResponseGroup.Nodes);

            //get the discount price and return the highest of the discounted price and base price
            Price discountedPrice = GetDiscountPriceInternal(contentReference, entry, lowPrice, null, null); // sending empty... for now

            // As starter have the fork but return null...in both
            //ToDO: Add logic to set the discounted price to the base price if its lower than the base price
            // starter
            //if (basePrice != null && discountedPrice != null)

            // Solution, messy rewritten to us the new "promo-system"
            //if (basePrice != null && discountedPrice != null && basePrice.UnitPrice.Amount
            if (basePrice != null && basePrice.UnitPrice.Amount
            //> discountedPrice.UnitPrice.Amount) // old promos
            > promoPrice) // new promos
            {
                return new Price(basePrice);
            }
            else
            {
                // returning the promo-Price ... need a re-work
                return new Price
                {
                    UnitPrice = new Money(promoPrice, _currentMarket.Service.GetCurrentMarket().DefaultCurrency),
                    ValidFrom = lowPrice.ValidFrom,
                    ValidUntil = lowPrice.ValidUntil,
                    MinQuantity = lowPrice.MinQuantity,
                    MarketId = lowPrice.MarketId,
                    EntryContent = contentReference,
                    CustomerPricing = lowPrice.CustomerPricing
                };

            }
        }

        // new method for the...
        // \Infrastructure\Promotions\CustomPromotionEngineContentLoader.cs
        internal static IPriceValue GetSalePrice(ContentReference entryLink)
        {
            // Need the pricing context... have a look here
            List<CustomerPricing> customerPricing = GetCustomerPricingList(); 
            IMarket theMarket = _currentMarket.Service.GetCurrentMarket();
            IEnumerable<Currency> currencies = theMarket.Currencies;

            PriceFilter filter = new PriceFilter()
            {
                Quantity = 1M, // can be improved, simple for now
                Currencies = currencies,
                CustomerPricing = customerPricing,
                ReturnCustomerPricing = false
            };

            VariationContent theEntry = _contentLoader.Service.Get<VariationContent>(entryLink);
            CatalogKey catalogKey = new CatalogKey(theEntry.Code); // 3 overloads


            //_pricingLoader.Service.GetPrices(entryLink,theMarket.MarketId.Value)
            IEnumerable<IPriceValue> prices = _priceService.Service.GetPrices(
                theMarket.MarketId.Value, FrameworkContext.Current.CurrentDateTime, catalogKey, filter);
            //_roPriceLoader.Service.GetCustomerPrices(contentReference,) // may use this one

            // Don't want the "BasePrice" ... this is the "SalePrice"
            return prices.Where(x =>
                x.CustomerPricing.PriceTypeId != (CustomerPricing.PriceType)3) 
                .OrderBy(pv => pv.UnitPrice).FirstOrDefault(); // Lowest price
            
            //return prices.OrderByDescending(p => p.UnitPrice.Amount).Last(); 
        }

        //internal static IPriceValue GetDiscountedPrice(ContentReference entryLink)
        //{
        //    List<CustomerPricing> customerPricing = GetCustomerPricingList();

        //    return null;
        //}

        // Populate a CustomerPricing List for the pricing retrieval

        // using this
        private static List<CustomerPricing> GetCustomerPricingList() // 
        {
            // Add the standard stuff and "base price" 
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
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup,
                        currentUserContact.EffectiveCustomerGroup));
                }
            }

            //ToDO: Add the new price type (can be empty)
            // Solution, don't need a starter
            // take this one out for the new promotions
            // This is the "BasePrice"
            customerPricing.Add(new CustomerPricing((CustomerPricing.PriceType)3, string.Empty));

            return customerPricing;
        }

        // Old stuff... no demo... Legacy Promos
        // This is a slightly-refactored version of the StoreHelper.GetDiscountPrice() method
        // catalogName and catalogNodeCode... can be used to filter out certain nodes or catalogs
        private static Price GetDiscountPriceInternal(EntryContentBase contentSku, Entry sku, IPriceValue price, string catalogName, string catalogNodeCode)
        {
            string catalogNodes = String.Empty;
            string catalogs = String.Empty;
            // Now cycle through all the catalog nodes where this entry is present filtering by specified catalog and node code
            // Note: The nodes are only populated when Full or Nodes response group is specified.
            if (sku.Nodes != null && sku.Nodes.CatalogNode != null && sku.Nodes.CatalogNode.Length > 0)
            {
                foreach (CatalogNode node in sku.Nodes.CatalogNode)
                {
                    string entryCatalogName = CatalogContext.Current.GetCatalogDto(node.CatalogId).Catalog[0].Name;

                    // Skip filtered catalogs
                    if (!String.IsNullOrEmpty(catalogName) && !entryCatalogName.Equals(catalogName))
                        continue;

                    // Skip filtered catalogs nodes
                    if (!String.IsNullOrEmpty(catalogNodeCode) && !node.ID.Equals(catalogNodeCode, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (String.IsNullOrEmpty(catalogs))
                        catalogs = entryCatalogName;
                    else
                        catalogs += ";" + entryCatalogName;

                    if (String.IsNullOrEmpty(catalogNodes))
                        catalogNodes = node.ID;
                    else
                        catalogNodes += ";" + node.ID;
                }
            }

            if (String.IsNullOrEmpty(catalogs))
                catalogs = catalogName;

            if (String.IsNullOrEmpty(catalogNodes))
                catalogNodes = catalogNodeCode;

            // Get current context
            Dictionary<string, object> context = MarketingContext.Current.MarketingProfileContext;

            // Create filter
            PromotionFilter filter = new PromotionFilter
            {
                IgnoreConditions = false,
                IgnorePolicy = false,
                IgnoreSegments = false,
                IncludeCoupons = false
            };

            // Create new entry
            // Note: catalogNodes is determined by GetParentNodes(entry)
            PromotionEntry result = new PromotionEntry(catalogs, catalogNodes, sku.ID, price.UnitPrice.Amount);
            var promotionEntryPopulateService =
                (IPromotionEntryPopulate)MarketingContext.Current.PromotionEntryPopulateFunctionClassInfo.CreateInstance();
            promotionEntryPopulateService.Populate(result, sku, _currentMarket.Service.GetCurrentMarket().MarketId,
                                                   _currentMarket.Service.GetCurrentMarket().DefaultCurrency);

            PromotionEntriesSet sourceSet = new PromotionEntriesSet();
            sourceSet.Entries.Add(result);

            // Create new promotion helper, which will initialize PromotionContext object for us and setup context dictionary
            PromotionHelper helper = new PromotionHelper();

            // Only target entries
            helper.PromotionContext.TargetGroup = PromotionGroup.GetPromotionGroup(PromotionGroup.PromotionGroupKey.Entry).Key;

            // Configure promotion context
            helper.PromotionContext.SourceEntriesSet = sourceSet;
            helper.PromotionContext.TargetEntriesSet = sourceSet;

            // Execute the promotions and filter out basic collection of promotions, we need to execute with cache disabled, so we get latest info from the database
            helper.Eval(filter);

            Money discountedAmount;
            // Check the count, and get new price if promotion is applied
            if (helper.PromotionContext.PromotionResult.PromotionRecords.Count > 0)
            {
                discountedAmount = new Money(price.UnitPrice.Amount -
                                                  GetDiscountPriceFromPromotionResult(
                                                      helper.PromotionContext.PromotionResult),
                   _currentMarket.Service.GetCurrentMarket().DefaultCurrency);

                return new Price
                {
                    UnitPrice = discountedAmount,
                    ValidFrom = price.ValidFrom,
                    ValidUntil = price.ValidUntil,
                    MinQuantity = price.MinQuantity,
                    MarketId = price.MarketId,
                    EntryContent = contentSku,
                    CustomerPricing = price.CustomerPricing
                };
            }
            else
            {
                return new Price(price);
            }

        }

        // Legacy gets the discount price... 
        private static decimal GetDiscountPriceFromPromotionResult(PromotionResult result)
        {
            decimal discountAmount = 0;
            foreach (PromotionItemRecord record in result.PromotionRecords)
            {
                discountAmount += GetDiscountAmount(record, record.PromotionReward);
            }

            return discountAmount;
        }

        // Legacy... gets the discount amount for one entry only... 
        private static decimal GetDiscountAmount(PromotionItemRecord record, PromotionReward reward)
        {
            decimal discountAmount = 0;
            if (reward.RewardType == PromotionRewardType.EachAffectedEntry || reward.RewardType == PromotionRewardType.AllAffectedEntries)
            {
                if (reward.AmountType == PromotionRewardAmountType.Percentage)
                {
                    discountAmount = record.AffectedEntriesSet.TotalCost * reward.AmountOff / 100;
                }
                else // need to split discount between all items
                {
                    discountAmount += reward.AmountOff; // since we assume only one entry in affected items
                }
            }
            return Math.Round(discountAmount, 2);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Engine.Navigation;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Objects;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Security;
using Mediachase.Commerce;
using Mediachase.Commerce.Website.Helpers;
namespace CommerceTraining.Infrastructure.Pricing
{
    public class CustomStoreHelper
    {
        // This class is old, will be removed

        /// <summary>
        /// Loads the payment plugin.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        public static Control LoadPaymentPlugin(UserControl control, string keyword)
        {
            System.Web.UI.Control paymentCtrl = null;
            string path = String.Concat(control.TemplateSourceDirectory, "/plugins/payment/", keyword, "/PaymentMethod.ascx");
            if (File.Exists(HttpContext.Current.Server.MapPath(path)))
            {
                paymentCtrl = control.LoadControl(path);
            }
            else
            {
                // Control not found, use generic one
                paymentCtrl = control.LoadControl(String.Format("{0}{1}Generic/PaymentMethod.ascx", control.TemplateSourceDirectory, "/plugins/payment/"));
            }

            paymentCtrl.ID = keyword;
            return paymentCtrl;
        }

        public static string GetAddressName(OrderAddress orderAddress)
        {
            var retVal = string.Empty;
            if (orderAddress != null)
            {
                retVal = orderAddress.FirstName + " " + orderAddress.LastName + " "
                         + orderAddress.Line1 + " " + orderAddress.Line2;
            }
            return retVal;
        }

        public static bool IsCustomerAddressAlreadyExistInContact(CustomerContact contact, CustomerAddress address)
        {
            var retVal = false;
            //Try to find already exist same address
            if (contact.ContactAddresses != null)
            {
                retVal = contact.ContactAddresses.Any(x => x.Equals(address));
            }
            return retVal;
        }

        /// <summary>
        /// Returns address formatted as a string
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        public static string GetAddressString(OrderAddress info)
        {
            if (info == null)
                return String.Empty;

            StringBuilder strAddress = new StringBuilder();

            strAddress.AppendFormat("<li>{0} {1}</li>", info.FirstName, info.LastName);

            if (!String.IsNullOrEmpty(info.Organization))
                strAddress.AppendFormat("<li>{0}</li>", info.Organization);

            if (!String.IsNullOrEmpty(info.Line1))
                strAddress.AppendFormat("<li>{0}</li>", info.Line1);

            if (!String.IsNullOrEmpty(info.Line2))
                strAddress.AppendFormat("<li>{0}</li>", info.Line2);

            if (!String.IsNullOrEmpty(info.City))
                strAddress.AppendFormat("<li>{0}, {1} {2}</li>", info.City, info.State, info.PostalCode);
            else if (!String.IsNullOrEmpty(info.State) && !String.IsNullOrEmpty(info.PostalCode))
                strAddress.AppendFormat("<li>{0} {1}</li>", info.State, info.PostalCode);

            if (!String.IsNullOrEmpty(info.CountryName))
                strAddress.AppendFormat("<li>{0}</li>", info.CountryName);

            if (!String.IsNullOrEmpty(info.DaytimePhoneNumber))
                strAddress.AppendFormat("<li>{0}</li>", info.DaytimePhoneNumber);

            if (!String.IsNullOrEmpty(info.EveningPhoneNumber))
                strAddress.AppendFormat("<li>{0}</li>", info.EveningPhoneNumber);

            if (!String.IsNullOrEmpty(info.FaxNumber))
                strAddress.AppendFormat("<li>{0}</li>", info.FaxNumber);

            return strAddress.ToString();
        }

        /// <summary>
        /// Gets the quantity as string.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        public static string GetQuantityAsString(decimal quantity)
        {
            if (Decimal.Round(quantity, 0) != quantity)
            {
                return quantity.ToString();
            }
            else
            {
                return Convert.ToInt32(quantity).ToString();
            }
        }

        /// <summary>
        /// Checks if customer address collection already contains the specified address.
        /// </summary>
        /// <param name="collection">Customer addresses collection (Profile.Account.Addresses).</param>
        /// <param name="address">Address to check.</param>
        /// <returns>True, if address is already in the collection.</returns>
        /// <remarks>Only address' properties are checked (like first, last name, city, state,...). Address name and addressId are ignored.
        /// </remarks>
        public static bool IsAddressInCollection(IEnumerable<CustomerAddress> collection, CustomerAddress address)
        {
            if (address == null)
                return false;

            bool found = false;

            foreach (CustomerAddress tmpAddress in collection)
            {
                if (CheckAddressesEquality(tmpAddress, address))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Checks if 2 customer addresses are the same (have same first, last names, city, state,...).
        /// Address ids and names are ignored.
        /// </summary>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        /// <returns></returns>
        public static bool CheckAddressesEquality(CustomerAddress address1, CustomerAddress address2)
        {
            bool addressesEqual = false;

            if (address1 == null && address2 == null)
                addressesEqual = true;
            else if (address1 == null && address2 != null)
                addressesEqual = false;
            else if (address1 != null && address2 == null)
                addressesEqual = false;
            else if (address1 != null && address2 != null)
            {
                addressesEqual = AddressStringsEqual(address1.City, address2.City) &&
                    AddressStringsEqual(address1.CountryCode, address2.CountryCode) &&
                    AddressStringsEqual(address1.CountryName, address2.CountryName) &&
                    AddressStringsEqual(address1.DaytimePhoneNumber, address2.DaytimePhoneNumber) &&
                    AddressStringsEqual(address1.EveningPhoneNumber, address2.EveningPhoneNumber) &&
                    //(String.Compare(address1.FaxNumber, address2.FaxNumber) == 0) &&
                    AddressStringsEqual(address1.FirstName, address2.FirstName) &&
                    AddressStringsEqual(address1.LastName, address2.LastName) &&
                    AddressStringsEqual(address1.Line1, address2.Line1) &&
                    AddressStringsEqual(address1.Line2, address2.Line2) &&
                    AddressStringsEqual(address1.Organization, address2.Organization) &&
                    AddressStringsEqual(address1.RegionCode, address2.RegionCode) &&
                    AddressStringsEqual(address1.RegionName, address2.RegionName) &&
                    AddressStringsEqual(address1.State, address2.State);
            }

            return addressesEqual;
        }

        private static bool AddressStringsEqual(string str1, string str2)
        {
            return (String.IsNullOrEmpty(str1) && String.IsNullOrEmpty(str2)) ||
                (String.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Converts to customer address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public static Mediachase.Commerce.Customers.CustomerAddress ConvertToCustomerAddress(OrderAddress address)
        {
            Mediachase.Commerce.Customers.CustomerAddress newAddress = CustomerAddress.CreateForApplication();

            newAddress.Name = GetAddressName(address);

            newAddress.City = address.City;
            newAddress.CountryCode = address.CountryCode;
            newAddress.CountryName = address.CountryName;
            newAddress.DaytimePhoneNumber = address.DaytimePhoneNumber;
            newAddress.Email = address.Email;
            newAddress.EveningPhoneNumber = address.EveningPhoneNumber;
            //newAddress.FaxNumber = address.FaxNumber;
            newAddress.FirstName = address.FirstName;
            newAddress.LastName = address.LastName;
            newAddress.Line1 = address.Line1;
            newAddress.Line2 = address.Line2;
            //newAddress.Organization = address.Organization;
            newAddress.PostalCode = address.PostalCode;
            newAddress.RegionName = address.RegionName;
            newAddress.RegionCode = address.RegionCode;
            newAddress.State = address.State;
            return newAddress;
        }

        /// <summary>
        /// Converts to order address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public static OrderAddress ConvertToOrderAddress(Mediachase.Commerce.Customers.CustomerAddress address)
        {
            OrderAddress newAddress = new OrderAddress();
            newAddress.City = address.City;
            newAddress.CountryCode = address.CountryCode;
            newAddress.CountryName = address.CountryName;
            newAddress.DaytimePhoneNumber = address.DaytimePhoneNumber;
            newAddress.Email = address.Email;
            newAddress.EveningPhoneNumber = address.EveningPhoneNumber;
            //newAddress.FaxNumber = address.FaxNumber;
            newAddress.FirstName = address.FirstName;
            newAddress.LastName = address.LastName;
            newAddress.Line1 = address.Line1;
            newAddress.Line2 = address.Line2;
            newAddress.Name = address.Name;
            //newAddress.Organization = address.Organization;
            newAddress.PostalCode = address.PostalCode;
            newAddress.RegionName = address.RegionName;
            newAddress.RegionCode = address.RegionCode;
            newAddress.State = address.State;
            return newAddress;
        }

        /// <summary>
        /// Gets the node URL.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetNodeUrl(CatalogNode node)
        {
            string url = String.Empty;
            Seo seo = GetLanguageSeo(node.SeoInfo);

            if (seo != null)
                url = "~/" + seo.Uri;

            return url;
        }

        /// <summary>
        /// Gets the entry URL.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static string GetEntryUrl(Entry entry)
        {
            var contentLink = ServiceLocator.Current.GetInstance<ReferenceConverter>().GetContentLink(entry.CatalogEntryId, CatalogContentType.CatalogEntry, 0);
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(contentLink);
        }

        /// <summary>
        /// Gets the language seo.
        /// </summary>
        /// <param name="seoInfo">The seo info.</param>
        /// <returns></returns>
        public static Seo GetLanguageSeo(Seo[] seoInfo)
        {
            Seo seoReturn = null;
            if (seoInfo != null)
            {
                foreach (Seo seo in seoInfo)
                {
                    if (seo.LanguageCode.Equals(ContentLanguage.PreferredCulture.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        seoReturn = seo;
                        break;
                    }
                }
            }

            return seoReturn;
        }

        /// <summary>
        /// Gets the sale price.
        /// </summary>
        /// <param name="entry">The entry used to fetch prices.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="market">The market.</param>
        /// <param name="currency">The currency.</param>
        /// <returns></returns>
        public static Price GetSalePrice(Entry entry, decimal quantity, IMarket market, Currency currency)
        {
            List<CustomerPricing> customerPricing = new List<CustomerPricing>();
            customerPricing.Add(CustomerPricing.AllCustomers);

            var principal = PrincipalInfo.CurrentPrincipal;
            if (principal != null)
            {
                if (!string.IsNullOrEmpty(principal.Identity.Name))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.UserName, principal.Identity.Name));
                }

                CustomerContact currentUserContact = principal.GetCustomerContact();
                if (currentUserContact != null && !string.IsNullOrEmpty(currentUserContact.EffectiveCustomerGroup))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup, currentUserContact.EffectiveCustomerGroup));
                }
            }

            IPriceService priceService = ServiceLocator.Current.GetInstance<IPriceService>();
            PriceFilter filter = new PriceFilter()
            {
                Quantity = quantity,
                Currencies = new Currency[] { currency },
                CustomerPricing = customerPricing
            };

            // return less price value
            IPriceValue priceValue = priceService.GetPrices(market.MarketId, FrameworkContext.Current.CurrentDateTime, new CatalogKey(entry), filter)
                .OrderBy(pv => pv.UnitPrice)
                .FirstOrDefault();

            if (priceValue != null)
            {
                return new Price(priceValue.UnitPrice);
            }

            return null;
        }

        /// <summary>
        /// Gets the sale price. The current culture info currency code will be used.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="market">Market for filtering.</param>
        /// <returns>Price</returns>
        public static Price GetSalePrice(Entry entry, decimal quantity, IMarket market)
        {
            return GetSalePrice(entry, quantity, market, market.DefaultCurrency);
        }

        /// <summary>
        /// Gets the sale price. The current culture info currency code will be used.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Price</returns>
        /// 
        public static Price GetSalePrice(Entry entry, decimal quantity)
        {
            ICurrentMarket currentMarketService = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            return GetSalePrice(entry, quantity, currentMarketService.GetCurrentMarket());
        }

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        //public static Price GetDiscountPrice(Entry entry)
        //{
        //    return GetDiscountPrice(entry, string.Empty, string.Empty);
        //}

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="catalogName">Name of the catalog.</param>
        /// <returns></returns>
        //public static Price GetDiscountPrice(Entry entry, string catalogName)
        //{
        //    return GetDiscountPrice(entry, catalogName, string.Empty);
        //}

        /// <summary>
        /// Gets the discounted price of the catalog entry.
        /// </summary>
        /// <param name="entry">The catalog entry.</param>
        /// <param name="catalogName">Name of the catalog to filter the discounts for. If null, all catalogs containing the entry will be used.</param>
        /// <param name="catalogNodeCode">The catalog node code to filter the discounts for. If null, all catalog nodes containing the entry will be used.</param>
        /// <param name="market">The market to fetch tiered base pricing for.</param>
        /// <returns>The discounted price of the catalog entry.</returns>
        /// <remarks>Uses market.DefaultCurrency for the currency.</remarks>
        //public static Price GetDiscountPrice(Entry entry, string catalogName, string catalogNodeCode, IMarket market)
        //{
        //    return GetDiscountPrice(entry, catalogName, catalogNodeCode, market, market.DefaultCurrency);
        //}

        /// <summary>
        /// Gets the discounted price of the catalog entry.
        /// </summary>
        /// <param name="entry">The catalog entry.</param>
        /// <param name="catalogName">Name of the catalog to filter the discounts for. If null, all catalogs containing the entry will be used.</param>
        /// <param name="catalogNodeCode">The catalog node code to filter the discounts for. If null, all catalog nodes containing the entry will be used.</param>
        /// <param name="market">The market to fetch tiered base pricing for.</param>
        /// <param name="currency">The currency to fetch prices in.</param>
        /// <returns>The discounted price of the catalog entry.</returns>
        //public static Price GetDiscountPrice(Entry entry, string catalogName, string catalogNodeCode, IMarket market, Currency currency)
        //{
        //    if (entry == null)
        //        throw new NullReferenceException("entry can't be null");

        //    decimal minQuantity = 1;

        //    // get min quantity attribute
        //    if (entry.ItemAttributes != null)
        //        minQuantity = entry.ItemAttributes.MinQuantity;

        //    // we can't pass qauntity of 0, so make it default to 1
        //    if (minQuantity <= 0)
        //        minQuantity = 1;

        //    // Get sale price for the current user
        //    Price price = StoreHelper.GetSalePrice(entry, minQuantity, market, currency);
        //    if (price == null)
        //    {
        //        return null;
        //    }

        //    string catalogNodes = String.Empty;
        //    string catalogs = String.Empty;
        //    // Now cycle through all the catalog nodes where this entry is present filtering by specified catalog and node code
        //    // The nodes are only populated when Full or Nodes response group is specified.
        //    if (entry.Nodes != null && entry.Nodes.CatalogNode != null && entry.Nodes.CatalogNode.Length > 0)
        //    {
        //        foreach (CatalogNode node in entry.Nodes.CatalogNode)
        //        {
        //            string entryCatalogName = CatalogContext.Current.GetCatalogDto(node.CatalogId).Catalog[0].Name;

        //            // Skip filtered catalogs
        //            if (!String.IsNullOrEmpty(catalogName) && !entryCatalogName.Equals(catalogName))
        //                continue;

        //            // Skip filtered catalogs nodes
        //            if (!String.IsNullOrEmpty(catalogNodeCode) && !node.ID.Equals(catalogNodeCode, StringComparison.OrdinalIgnoreCase))
        //                continue;

        //            if (String.IsNullOrEmpty(catalogs))
        //                catalogs = entryCatalogName;
        //            else
        //                catalogs += ";" + entryCatalogName;

        //            if (String.IsNullOrEmpty(catalogNodes))
        //                catalogNodes = node.ID;
        //            else
        //                catalogNodes += ";" + node.ID;
        //        }
        //    }

        //    if (String.IsNullOrEmpty(catalogs))
        //        catalogs = catalogName;

        //    if (String.IsNullOrEmpty(catalogNodes))
        //        catalogNodes = catalogNodeCode;

        //    // Get current context
        //    Dictionary<string, object> context = MarketingContext.Current.MarketingProfileContext;

        //    // Create filter
        //    PromotionFilter filter = new PromotionFilter();
        //    filter.IgnoreConditions = false;
        //    filter.IgnorePolicy = false;
        //    filter.IgnoreSegments = false;
        //    filter.IncludeCoupons = false;

        //    // Create new entry
        //    // TPB: catalogNodes is determined by the front end. GetParentNodes(entry)
        //    PromotionEntry result = new PromotionEntry(catalogs, catalogNodes, entry.ID, price.Money.Amount);
        //    var promotionEntryPopulateService = (IPromotionEntryPopulate)MarketingContext.Current.PromotionEntryPopulateFunctionClassInfo.CreateInstance();
        //    promotionEntryPopulateService.Populate(result, entry, market.MarketId, currency);

        //    PromotionEntriesSet sourceSet = new PromotionEntriesSet();
        //    sourceSet.Entries.Add(result);

        //    return GetDiscountPrice(filter, price, sourceSet, sourceSet);
        //}

        /// <summary>
        /// Gets the discount price by evaluating the discount rules and taking into account segments customer belongs to.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="catalogName">Name of the catalog to filter the discounts for. If null, all catalogs entry belongs to will be applied.</param>
        /// <param name="catalogNodeCode">The catalog node code to filter the discounts for. If null, all catalog nodes entry belongs to will be applied.</param>
        /// <returns></returns>
        //public static Price GetDiscountPrice(Entry entry, string catalogName, string catalogNodeCode)
        //{
        //    ICurrentMarket currentMarketService = ServiceLocator.Current.GetInstance<ICurrentMarket>();

        //    return GetDiscountPrice(entry, catalogName, catalogNodeCode, currentMarketService.GetCurrentMarket());
        //}

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="salePrice">The sale price.</param>
        /// <param name="sourceSet">The source set.</param>
        /// <param name="targetSet">The target set.</param>
        /// <returns></returns>
        //private static Price GetDiscountPrice(PromotionFilter filter, Price salePrice, PromotionEntriesSet sourceSet, PromotionEntriesSet targetSet)
        //{
        //    // Create new promotion helper, which will initialize PromotionContext object for us and setup context dictionary
        //    CustomPromotionHelper helper = new CustomPromotionHelper();

        //    // Only target entries
        //    helper.PromotionContext.TargetGroup = PromotionGroup.GetPromotionGroup(PromotionGroup.PromotionGroupKey.Entry).Key;

        //    // Configure promotion context
        //    helper.PromotionContext.SourceEntriesSet = sourceSet;
        //    helper.PromotionContext.TargetEntriesSet = targetSet;

        //    // Execute the promotions and filter out basic collection of promotions, we need to execute with cache disabled, so we get latest info from the database
        //    helper.Eval(filter);

        //    // Check the count, and get new price
        //    if (helper.PromotionContext.PromotionResult.PromotionRecords.Count > 0)  // don't get anything here
        //        return ObjectHelper.CreatePrice(new Money(salePrice.Money.Amount - GetDiscountPrice(helper.PromotionContext.PromotionResult), salePrice.Money.Currency));
        //    else
        //        return salePrice;
        //}

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private static decimal GetDiscountPrice(PromotionResult result)
        {
            decimal discountAmount = 0;
            foreach (PromotionItemRecord record in result.PromotionRecords)
            {
                discountAmount += GetDiscountAmount(record, record.PromotionReward);
            }

            return discountAmount;
        }

        /// <summary>
        /// Gets the discount amount for one entry only.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="reward">The reward.</param>
        /// <returns></returns>
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

        #region Inventory Helpers
        /// <summary>
        /// Determines whether entry is in stock.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// 	<c>true</c> if entry is in stock otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInStock(Entry entry)
        {
            if (entry == null)
                return false;

            if (entry.WarehouseInventories == null)
                return false;

            IWarehouseInventory sumInventory = SumInventories(entry.WarehouseInventories.WarehouseInventory);

            // If we don't account inventory return true always
            if (sumInventory.InventoryStatus != InventoryTrackingStatus.Enabled)
            {
                return true;
            }

            return (GetItemsInStock(entry) > 0) ? true : false;
        }

        /// <summary>
        /// Gets the items in stock.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static decimal GetItemsInStock(Entry entry)
        {
            if (entry == null)
                return 0;

            if (entry.WarehouseInventories == null)
                return 0;

            IWarehouseInventory sumInventory = SumInventories(entry.WarehouseInventories.WarehouseInventory);
            return sumInventory.InStockQuantity - sumInventory.ReservedQuantity;
        }

        /// <summary>
        /// Gets the inventory status.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static string GetInventoryStatus(Entry entry)
        {
            if (IsInStock(entry))
                return "Item is in stock";

            if (entry == null)
                return "";

            if (entry.WarehouseInventories == null)
                return "";

            IWarehouseInventory sumInventory = SumInventories(entry.WarehouseInventories.WarehouseInventory);

            if (IsAvailableForPreorder(entry))
                return "Item is available for preorder";
            else if (sumInventory.AllowPreorder && sumInventory.PreorderAvailabilityDate.HasValue)
                return String.Format("Item will be available for preorder on {0}", sumInventory.PreorderAvailabilityDate.Value.ToString("MMMM dd, yyyy"));

            if (IsAvailableForBackorder(entry))
                return "Item is available for backorder";
            else if (sumInventory.AllowBackorder && sumInventory.BackorderAvailabilityDate.HasValue)
                return String.Format("Item will be available for backorder on {0}", sumInventory.BackorderAvailabilityDate.Value.ToString("MMMM dd, yyyy"));

            return "Item is out of stock";
        }

        /// <summary>
        /// Determines whether [is available for backorder] [the specified entry].
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// 	<c>true</c> if [is available for backorder] [the specified entry]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAvailableForBackorder(Entry entry)
        {
            if (entry == null)
                return false;

            if (entry.WarehouseInventories == null)
                return false;

            IWarehouseInventory sumInventory = SumInventories(entry.WarehouseInventories.WarehouseInventory);

            if (sumInventory.AllowBackorder && sumInventory.BackorderQuantity > 0 && sumInventory.BackorderAvailabilityDate <= DateTime.UtcNow)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether [is available for preorder] [the specified entry].
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        /// 	<c>true</c> if [is available for preorder] [the specified entry]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAvailableForPreorder(Entry entry)
        {
            if (entry == null)
                return false;

            if (entry.WarehouseInventories == null)
                return false;

            IWarehouseInventory sumInventory = SumInventories(entry.WarehouseInventories.WarehouseInventory);

            if (sumInventory.AllowPreorder && sumInventory.PreorderQuantity > 0 && sumInventory.PreorderAvailabilityDate <= DateTime.UtcNow)
                return true;

            return false;
        }

        /// <summary>
        /// Gets all item in stock.
        /// </summary>
        /// <param name="inventories"> The WarehouseInventory.</param>
        /// <returns></returns>
        public static IWarehouseInventory SumInventories(IEnumerable<IWarehouseInventory> inventories)
        {
            WarehouseInventory result = new WarehouseInventory()
            {
                InStockQuantity = 0,
                ReservedQuantity = 0,
                ReorderMinQuantity = 0,
                PreorderQuantity = 0,
                BackorderQuantity = 0,
                AllowBackorder = false,
                AllowPreorder = false,
                PreorderAvailabilityDate = DateTime.MaxValue,
                BackorderAvailabilityDate = DateTime.MaxValue
            };
            IWarehouseRepository warehouseRepository = EPiServer.ServiceLocation.ServiceLocator.Current.GetInstance<IWarehouseRepository>();

            foreach (IWarehouseInventory inventory in inventories)
            {
                if (warehouseRepository.Get(inventory.WarehouseCode).IsActive)
                {
                    // Sum up quantity fields
                    result.BackorderQuantity += inventory.BackorderQuantity;
                    result.InStockQuantity += inventory.InStockQuantity;
                    result.PreorderQuantity += inventory.PreorderQuantity;
                    result.ReorderMinQuantity += inventory.ReorderMinQuantity;
                    result.ReservedQuantity += inventory.ReservedQuantity;

                    // Check flags that should be global when aggregating warehouse inventories
                    result.AllowBackorder = inventory.AllowBackorder ? inventory.AllowBackorder : result.AllowBackorder;
                    result.AllowPreorder = inventory.AllowPreorder ? inventory.AllowPreorder : result.AllowPreorder;

                    result.BackorderAvailabilityDate = GetAvailabilityDate(result.BackorderAvailabilityDate, inventory.BackorderAvailabilityDate);
                    result.PreorderAvailabilityDate = GetAvailabilityDate(result.PreorderAvailabilityDate, inventory.PreorderAvailabilityDate);
                }
            }

            return result;
        }

        private static DateTime? GetAvailabilityDate(DateTime? resultDate, DateTime? originalDate)
        {
            if (resultDate.HasValue && originalDate.HasValue)
            {
                return DateTime.Compare(resultDate.Value, originalDate.Value) < 0 ? resultDate : originalDate;
            }

            return resultDate;
        }

        #endregion

        /// <summary>
        /// Gets the display name of the entry. Returns localized version or the product name if no localized version available.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static string GetEntryDisplayName(Entry entry)
        {
            string name = entry.Name;
            if (entry.ItemAttributes != null && entry.ItemAttributes["DisplayName"] != null)
            {
                string displayName = entry.ItemAttributes["DisplayName"].ToString();
                if (!String.IsNullOrEmpty(displayName))
                    name = displayName;
            }

            return name;
        }

        /// <summary>
        /// Gets the display name of the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetNodeDisplayName(CatalogNode node)
        {
            string name = node.Name;
            if (node.ItemAttributes != null && node.ItemAttributes["DisplayName"] != null)
            {
                string displayName = node.ItemAttributes["DisplayName"].ToString();
                if (!String.IsNullOrEmpty(displayName))
                    name = displayName;
            }

            return name;
        }

        #region Browse History Management
        /// <summary>
        /// Adds the browse history.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddBrowseHistory(string key, string value)
        {
            int maxHistory = 10;
            NameValueCollection history = GetBrowseHistory();

            string[] values = history.GetValues(key);

            // Remove current key, since we will need to readd it
            history.Remove(key);

            if (values != null)
            {
                List<string> list = new List<string>(values);

                // Remove all items
                while (list.Remove(value)) ;

                // Remove oldest item(s)
                while (list.Count >= maxHistory)
                    list.RemoveAt(list.Count - 1);

                // Add it at the very front, since it is already sorted
                list.Insert(0, value);

                for (int index = 0; index < list.Count; index++)
                {
                    history.Add(key, list[index]);
                }
            }
            else
            {
                history.Add(key, value);
            }

            CommonHelper.SetCookie("BrowseHistory", history, DateTime.Now.AddDays(1));
        }

        /// <summary>
        /// Gets the browse history.
        /// </summary>
        /// <returns></returns>
        public static NameValueCollection GetBrowseHistory()
        {
            NameValueCollection cookie = CommonHelper.GetCookie("BrowseHistory");
            if (cookie == null)
                cookie = new NameValueCollection();
            return cookie;
        }
        #endregion
        /// <summary>
        /// Gets the discount price by current market.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static Price GetBasePrice(Entry entry)
        {
            return GetBasePrice(entry, 1);
        }
        /// <summary>
        /// Gets the current price by current market with min quantity.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="minQuantity">The min quantity.</param>
        /// <returns></returns>
        public static Price GetBasePrice(Entry entry, int minQuantity)
        {
            ICurrentMarket currentMarketService = ServiceLocator.Current.GetInstance<ICurrentMarket>();

            var currentMarketId = currentMarketService.GetCurrentMarket().MarketId;

            var priceValue = entry.PriceValues.PriceValue.Where(p => p.MinQuantity <= minQuantity && p.MarketId.Equals(currentMarketId) && IsActivePrice(p))
                .OrderByDescending(p => p.MinQuantity).FirstOrDefault();
            return priceValue == null ? null : new Price(priceValue.UnitPrice);
        }

        /// <summary>
        /// Determines whether price is active is not.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <returns>
        /// 	<c>true</c> if price is active otherwise, <c>false</c>.
        /// </returns>
        private static bool IsActivePrice(PriceValue price)
        {
            return price.ValidFrom < DateTime.Now && price.ValidUntil > DateTime.Now;
        }

    }
}
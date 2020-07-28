using CommerceTraining.SupportingClasses;
using EPiServer.Core;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
//using EPiServer.Commerce.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class ShirtVariationViewModel
    {
        // Markets
        public string currentMarket { get; set; }
        public string marketOwner { get; set; }
        // Pricing
        public decimal betaDiscountPrice { get; set; }
        public string PromoString { get; set; }
        public Price discountPrice { get; set; }

        public decimal discountPriceNew { get; set; }
        public string priceString { get; set; } // default?

        // can get the older "Price" if no qualified
        public EPiServer.Commerce.SpecializedProperties.Price CustomerPricingPrice { get; set; }

        public IEnumerable<IPriceValue> overridePrices { get; set; }
        public EPiServer.Commerce.SpecializedProperties.Price labPrice { get; set; }

        public string image { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool CanBeMonogrammed { get; set; }
        public XhtmlString MainBody { get; set; }

        public string CartUrl { get; set; }
        public string WishlistUrl { get; set; }

        public ContentArea ProductArea { get; set; }



        public IEnumerable<IWarehouseInventory> WHOldSchool { get; set; } // not using custom WarehouseInfo-class... yet
        public IEnumerable<InventoryRecord> WHNewSchool { get; set; }

        // new stuff: WH coding
        public IEnumerable<string> generalWarehouseInfo { get; set; }
        public IEnumerable<string> specificWarehouseInfo { get; set; }
        public IEnumerable<string> localMarketWarehouses { get; set; }
        public string entryCode { get; set; }

        // new stuff: Associations
        public IEnumerable<ContentReference> Associations { get; set; }
        public string AssociationMetaData { get; set; }
        public Dictionary<string, ContentReference> AssocAggregated { get; set; }

        // new stuff: BoughtThisBoughtThat
        public IEnumerable<string> BoughtThisBoughtThat { get; set; }

        // Taxes
        public IEnumerable<string> TaxString { get; set; }
        public decimal Tax { get; set; }

        public string TaxNewSchool { get; set; }

        // To get variation messages to the page
        public string VariationInfo { get; set; }

        public bool VariationAvailability { get; set; }
    }
}
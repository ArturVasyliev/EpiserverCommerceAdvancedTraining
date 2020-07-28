using EPiServer.Core;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class TrousersViewModel
    {

        public Price discountPrice { get; set; }
        public string priceString { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool CanBeMonogrammed { get; set; }
        public XhtmlString MainBody { get; set; }

        public string CartUrl { get; set; }
        public string WishlistUrl { get; set; }

        // copied from Adv.8.12
        public ContentArea ProductArea { get; set; }

        public IEnumerable<IWarehouseInventory> WHOldSchool { get; set; } // not using custom WarehouseInfo-class... yet
        public IEnumerable<InventoryRecord> WHNewSchool { get; set; }

        // new stuff: WH coding
        public IEnumerable<string> generalWarehouseInfo { get; set; }
        public IEnumerable<string> specificWarehouseInfo { get; set; }
        public string entryCode { get; set; }

        // new stuff: Associations
        public IEnumerable<ContentReference> Associations { get; set; }
        public string AssociationMetaData { get; set; }
        public Dictionary<string, ContentReference> AssocAggregated { get; set; }

        // new stuff: BoughtThisBoughtThat
        public IEnumerable<string> BoughtThisBoughtThat { get; set; }
    }
}
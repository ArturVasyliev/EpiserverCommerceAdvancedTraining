using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class DemoPromoViewModel
    {
        public List<CatItem> CatalogItems { get; set; }
        public IEnumerable<RewardDescription> Rewards { get; set; }

        public ICollection<ILineItem> CartItems { get; set; }

        public List<PromotionInformationEntry> PromoItems { get; set; }

        public DemoPromoViewModel()
        {
            CatalogItems = new List<CatItem>();
        }
    }

    public class CatItem
    {
        public string Code { get; set; }
        public int Quantity { get; set; }
    }
}
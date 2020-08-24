using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.DataAnnotations;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Infrastructure.Promotions
{
    [ContentType(DisplayName = "FreeStuffPromotion", GUID = "7ef6670e-5136-4dd4-bdb2-83646ebc64ff", Description = "")]
    public class FreeStuffPromotion : EntryPromotion
    {
        [PromotionRegion(PromotionRegionName.Condition)]
        [Display(Order = 20)]
        public virtual PurchaseQuantity RequiredQty { get; set; }

        [PromotionRegion(PromotionRegionName.Reward)]
        [Display(Order = 30)]
        public virtual IList<ContentReference> FreeItem { get; set; }

    }
}
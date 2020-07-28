using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Commerce.Marketing.DataAnnotations;

namespace CommerceTraining.Models.Promotions
{
    [ContentType(DisplayName = "FreeItemDiscount", GUID = "cc58d642-8a98-48d6-874e-04a0071dbfa5", Description = "")]
    public class FreeItemPromotion : EntryPromotion
    {
        [PromotionRegion("Condition")]
        [Display(Order = 10)]
        public virtual DiscountItems DiscountTargets { get; set; } // don't need this, just showing

        [PromotionRegion("Condition")]
        [Display(Order = 20)]
        public virtual PurchaseQuantity RequiredQty { get; set; }

        [PromotionRegion("Reward")]
        [Display(Order = 30)]
        public virtual DiscountItems FreeItem { get; set; }

        public virtual int PartialFulfillmentNumberOfItems { get; set; } // later addition
    }
}
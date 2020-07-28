using EPiServer.Commerce.Marketing.DataAnnotations;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Marketing; // Entry/Order/Shipping - baseclasses
using CommerceTraining.Infrastructure.Promotions; 

namespace CommerceTraining.Models.Promotions
{
    [ContentType(DisplayName = "MyPercentagePromotion"
    , GUID = "d255c44e-392a-4b0b-8fc2-a4402eda07ff", Description = "LineItems promotion")]
    public class MyPercentagePromotion : EntryPromotion
    {
        // Have MyPromotionGroupNames to have more options
        // ...Infrastructure\Promotions\MyPromotionGroupNames.cs
        
        [PromotionRegion(PromotionRegionName.Discount)]
        [Display(Order = -10, GroupName = MyPromotionGroupNames.GroupForDiscountTab
            , Description = "does not show")]
        public virtual MonetaryReward PercentageDiscount { get; set; }

        // when using PurchaseQuantity extension-methods come alive
        [PromotionRegion(PromotionRegionName.Condition)]
        [Display(Order = 10, GroupName = MyPromotionGroupNames.GroupForNumbersTab
            , Name = "Minimum number of items", Description = "does not show")]
        public virtual PurchaseQuantity MinNumberOfItems { get; set; }

        [PromotionRegion(PromotionRegionName.Reward)]
        [Display(Order = 20, GroupName = MyPromotionGroupNames.GroupForNumbersTab
            , Name = "Partial fulfillment number", Description = "does show")]
        public virtual int PartialFulfillmentNumberOfItems { get; set; } // later addition

        // out of the box...
        [PromotionRegion(PromotionRegionName.Reward)]
        [Display(Order = 30)]
        public virtual DiscountItems DiscountTargets { get; set; }
    }
}
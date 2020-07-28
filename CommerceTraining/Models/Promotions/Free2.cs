using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.DataAnnotations;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.Promotions
{
    [ContentType(DisplayName = "FreeItem2", GUID = "cc58d642-8a98-48d6-874e-04a0071dbfa6", Description = "")]
    public class Free2 : EntryPromotion
    {
        [PromotionRegion("Condition")]
        [Display(Order = 20)]
        public virtual PurchaseQuantity RequiredQty { get; set; }

        [PromotionRegion("Reward")]
        [Display(Order = 30)]
        public virtual DiscountItems FreeItem { get; set; }
    }
}
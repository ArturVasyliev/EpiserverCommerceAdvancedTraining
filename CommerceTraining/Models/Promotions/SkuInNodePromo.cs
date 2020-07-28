using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Marketing;
using System.Collections.Generic;

namespace CommerceTraining.Models.Promotions
{
    [ContentType(DisplayName = "SkuInNodePromo"
        , GUID = "d255c44e-392a-4b0b-8fc2-a4402eda07ee", Description = "Entry and LineItem")]
    public class SkuInNodePromo : EntryPromotion
    {
        public virtual IList<ContentReference> targetNodes { get; set; }
        public virtual int percentOff { get; set; }
    }
}
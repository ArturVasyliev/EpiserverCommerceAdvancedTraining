using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Commerce;

namespace CommerceTraining.Models.Blocks
{
    [ContentType(DisplayName = "SettingsBlock"
        , GUID = "fdfd33be-91ca-4366-a3ac-ea126c66f0e7"
        , Description = ""
        )]
    public class SettingsBlock : BlockData
    {
        [UIHint(UIHint.CatalogContent)]
        public virtual ContentReference topCategory { get; set; }

        public virtual ContentReference cartPage { get; set; }

        public virtual ContentReference checkoutPage { get; set; }

        public virtual ContentReference orderPage { get; set; }

        public virtual ContentReference catalogStartPageLink { get; set; }

        [UIHint(EPiServer.Commerce.UIHint.CatalogNode)]
        public virtual ContentReference WeeklySpecials { get; set; }

    }
}
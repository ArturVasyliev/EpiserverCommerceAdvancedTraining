using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;

namespace CommerceTraining.Models.Catalog // Added for Adv. ... as a common base
{
    [CatalogContentType(MetaClassName="Default_Variation"
        ,DisplayName = "DefaultVariation", GUID = "0f66b46e-248e-4c10-a6c3-f4979c9b4c6a", Description = "")]
    public class DefaultVariation : VariationContent
    {
        // need this for Adv. --> Forced Shipping
        [Display(Name="Special Shipment",Order=-10)]
        public virtual bool RequireSpecialShipping { get; set; }

        #region Dictionaries

        // ...do in "Advanced" (.Commerce.SpecializedProperties)
        [BackingType(typeof(PropertyDictionaryMultiple))]
        public virtual ItemCollection<string> MultiDict { get; set; }

        [BackingType(typeof(PropertyDictionarySingle))]
        public virtual string SingleDict { get; set; }

        //[BackingType(typeof(...not yet support for StringDictionaries))]
        //public virtual string StringDict { get; set; }

        #endregion

        [IncludeInDefaultSearch]
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 100)]
        public virtual XhtmlString MainBody { get; set; }


    
    }
}
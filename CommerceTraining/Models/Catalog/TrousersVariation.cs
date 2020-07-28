using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName="Trousers_Variation",DisplayName = "TrousersVariation", GUID = "d2489a87-5840-4661-8e20-a10e446dec56", Description = "")]
    public class TrousersVariation : DefaultVariation
    {
        //[IncludeInDefaultSearch]
        //        [CultureSpecific]
        //        [Display(
        //            Name = "Main body",
        //            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
        //            GroupName = SystemTabNames.Content,
        //            Order = 1)]
        //        public virtual XhtmlString MainBody { get; set; }

        // added for "adv"
        //public virtual bool RequireSpecialShipping { get; set; }
    }
}
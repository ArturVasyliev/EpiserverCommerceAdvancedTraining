using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName = "Blouse_Product", DisplayName = "BlouseProduct", GUID = "2b2ede7f-e5d8-4fa9-87bc-36f3f457c2ab", Description = "")]
    public class BlouseProduct : ProductContent
    {
        [IncludeInDefaultSearch]
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

    }
}
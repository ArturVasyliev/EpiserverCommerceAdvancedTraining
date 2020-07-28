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
    [CatalogContentType(DisplayName = "PoductLine", GUID = "14c9f905-e509-41f6-9bf6-1e06247de9ac", Description = "")]
    [AvailableContentTypes(Include = new Type[] { typeof(ProductColor) })]
    public class PoductLine : ProductContent
    {
        /*
                [CultureSpecific]
                [Display(
                    Name = "Main body",
                    Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
                    GroupName = SystemTabNames.Content,
                    Order = 1)]
                public virtual XhtmlString MainBody { get; set; }
         */
    }
}
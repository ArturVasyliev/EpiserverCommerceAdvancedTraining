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
    [CatalogContentType(DisplayName = "ProductColor", GUID = "f9d7072b-d5a2-41c9-b773-a9b025a6d4eb", Description = "")]
    [AvailableContentTypes(Include = new Type[] { typeof(ProductSizeSku) })]
    public class ProductColor : ProductContent
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
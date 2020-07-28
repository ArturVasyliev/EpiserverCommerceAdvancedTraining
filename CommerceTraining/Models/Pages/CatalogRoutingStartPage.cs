using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Pages
{
    [ContentType(DisplayName = "CatalogRoutingStartPage", GUID = "637986eb-6683-49b8-b0fe-02380eb82e86", Description = "")]
    public class CatalogRoutingStartPage : PageData
    {

        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

        //[UIHint(EPiServer.Web.UIHint.]
        public virtual ContentReference PromoPage { get; set; }

        // Checking on ECF & Blocks & CMS-pages
        public virtual ContentArea ProductArea { get; set; }

    }
}
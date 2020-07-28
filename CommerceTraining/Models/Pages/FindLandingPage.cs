using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Pages
{
    [ContentType(DisplayName = "FindLandingPage", GUID = "eabf00eb-2efd-455b-9e51-3c554f89edd4", Description = "")]
    public class FindLandingPage : PageData
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


        public virtual String Heading { get; set; }

        public virtual String Description { get; set; }

        public virtual XhtmlString MainBody { get; set; }

        public virtual ContentArea MainContentArea { get; set; }

        public virtual String SearchQuery { get; set; }
    }
}
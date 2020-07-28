using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Pages
{
    [ContentType(DisplayName = "PromotionPage", GUID = "2396b490-f762-48a2-98ea-b9c4b4a796a1", Description = "")]
    public class PromotionPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

        public virtual ContentArea CampaignArea { get; set; }

    }
}
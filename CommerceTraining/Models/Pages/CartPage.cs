using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Pages
{
    [ContentType(DisplayName = "CartPage", GUID = "5ccd975b-7af5-4231-aee7-d4ecdea51245"
        , Description = "Showing the cart")]
    public class CartPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body Cart Page can be used with the XHTML-editor for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBodyCartPage { get; set; }
    }
}
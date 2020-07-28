using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    // Std. routing
    public class BlouseProductViewModel : CatalogViewModel<BlouseProduct, CatalogRoutingStartPage>
    {
        public IEnumerable<EntryContentBase> productVariations { get; set; }
        public ContentReference campaignLink { get; set; }

        // Standard routing
        public BlouseProductViewModel(BlouseProduct currentContent, CatalogRoutingStartPage currentPage)
            : base(currentContent, currentPage)
        {
        }
    }
}
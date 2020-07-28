using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class CatalogViewModel<TCatalogContent, TPageData> : PageViewModel<TPageData>
        where TCatalogContent : CatalogContentBase
        where TPageData : PageData
    {
        public TCatalogContent CurrentContent { get; set; }

        public CatalogViewModel(TCatalogContent currentContent, TPageData currentPage)
            : base(currentPage)
        {
            CurrentContent = currentContent;
        }
    }
}
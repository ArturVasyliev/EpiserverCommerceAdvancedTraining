using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class PageViewModel<T> where T : PageData
    {

        public IEnumerable<IContent> topLevelCategories { get; set; }
        public IEnumerable<IContent> myPageChildren { get; set; }
        public virtual XhtmlString MainBodyStartPage { get; set; }
        public string Customer { get; set; }
        public IEnumerable<IMarket> markets { get; set; }
        public string selectedMarket { get; set; }
        public IEnumerable<string> someInfo { get; set; }

        public PageViewModel(T currentPage)
        {
            CurrentPage = currentPage;
        }

        public T CurrentPage
        {
            get;
            set;
        }
    }
}
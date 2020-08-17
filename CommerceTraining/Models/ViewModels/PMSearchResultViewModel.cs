using Mediachase.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class PMSearchResultViewModel
    {
        public string SearchQueryText { get; set; }
        public string ResultCount { get; set; }
        public List<ISearchDocument> SearchResults { get; set; }
        public List<ISearchFacetGroup> FacetGroups { get; set; }
    }
}
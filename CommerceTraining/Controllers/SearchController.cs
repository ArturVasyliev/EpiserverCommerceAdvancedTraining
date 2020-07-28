using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search.Extensions;
using Mediachase.Search;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using System;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using CommerceTraining.Models.Catalog;
using EPiServer.Globalization;
using CommerceTraining.SupportingClasses;
using EPiServer.Find;
using EPiServer.Find.Framework;
using Mediachase.Commerce.Core;
using System.IO;
using System.Xml.Serialization;

namespace CommerceTraining.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        public IEnumerable<IContent> localContent { get; set; }
        public readonly IContentLoader _contentLoader;
        public readonly ReferenceConverter _referenceConverter;
        public readonly UrlResolver _urlResolver;

        public SearchController(IContentLoader contentLoader, ReferenceConverter referenceConverter, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
        }

        public ActionResult Index(SearchPage currentPage)
        {
            var model = new SearchPageViewModel
            {
                CurrentPage = currentPage,
            };

            return View(model);
        }

        public ActionResult Search(string keyWord)
        {
            // ToDo: SearchHelper and Criteria 
            SearchFilterHelper searchHelper = SearchFilterHelper.Current; // the easy way

            CatalogEntrySearchCriteria criteria = searchHelper.CreateSearchCriteria(keyWord
                , CatalogEntrySearchCriteria.DefaultSortOrder);

            criteria.RecordsToRetrieve = 25;
            criteria.StartingRecord = 0;
            //criteria.Locale = "en"; // needed
            criteria.Locale = ContentLanguage.PreferredCulture.Name;

            int count = 0; // "Out"
            bool cacheResult = true;
            TimeSpan timeSpan = new TimeSpan(0, 10, 0);

            // ToDo: Search 
            // One way of "doing it" ... retrieve it like ISearchResults (preferred, most certainly)
            ISearchResults searchResult = searchHelper.SearchEntries(criteria);
            ISearchDocument aDoc = searchResult.Documents.FirstOrDefault();
            int[] ints = searchResult.GetKeyFieldValues<int>();

            /*
            // ECF style Entries, old-school & legacy, not recommended... 
            // ...work with DTOs if not using the ContentModel
            Entries entries = CatalogContext.Current.GetCatalogEntries(ints // Note "ints"
                , new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo));

            // Same thing ECF, old-style, not recommended... if not absolutely needed...
            // Use the helper and get the entries direct 
            // If entries are needed ... like for calculating discounts with StoreHelper()
            Entries entriesDirect = searchHelper.SearchEntries(criteria, out count // Note the different return-types ... akward!
                , new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo)
                , cacheResult, new TimeSpan());
            */

            // CMS style (better)... using ReferenceConverter and ContentLoader 
            List<ContentReference> refs = new List<ContentReference>();
            ints.ToList().ForEach(i => refs.Add(_referenceConverter.GetContentLink(i, CatalogContentType.CatalogEntry, 0)));

            localContent = _contentLoader.GetItems(refs, new LoaderOptions()); // use this in CMS 8+

            // ToDo: Facets
            List<string> facetList = new List<string>();

            int facetGroups = searchResult.FacetGroups.Count();

            foreach (ISearchFacetGroup item in searchResult.FacetGroups)
            {
                foreach (var item2 in item.Facets)
                {
                    facetList.Add(String.Format("{0} {1} ({2})", item.Name, item2.Name, item2.Count));
                }
            }

            // ToDo: As a last step - un-comment and fill up the ViewModel
            var searchResultViewModel = new SearchResultViewModel();

            searchResultViewModel.totalHits = new List<string> { "" }; // change
            searchResultViewModel.nodes = localContent.OfType<FashionNode>();
            searchResultViewModel.products = localContent.OfType<ShirtProduct>();
            searchResultViewModel.variants = localContent.OfType<ShirtVariation>();
            searchResultViewModel.allContent = localContent;
            searchResultViewModel.facets = facetList;


            return View(searchResultViewModel);
        }

        public ActionResult FindIntegrated(string keyWord)
        {
            IClient client = SearchClient.Instance;
            FindQueries Qs = new FindQueries(client);
            Qs.GetIntegrated(keyWord);

            return null;
        }

        public ActionResult FindNative(string keyWord)
        {
            IClient client = Client.CreateFromConfig();
            FindQueries Qs = new FindQueries(client, true);
            Qs.GetNative(keyWord);

            return null; // ...for now
        }

        public ActionResult FindUnified(string keyWord)
        {
            var model = new SearchResultViewModel
            {
                SearchQuery = keyWord,
                Results = SearchClient.Instance.UnifiedSearchFor(keyWord).ApplyBestBets().GetResult()
            };

            return View(model);
        }

        public ActionResult FindExtras(string keyWord)
        {
            IClient client = SearchClient.Instance;
            FindQueries Qs = new FindQueries(client);

            ContentReference theRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            ShirtVariation theVariant = _contentLoader.Get<ShirtVariation>(theRef);

            //Qs.SDKExamples(theRef);

            return null;
        }

        #region ProviderModelQueries 

        private static volatile SearchConfig _SearchConfig = null;
        public ActionResult ProviderModelQuery(string keyWord)
        {

            // Create criteria
            CatalogEntrySearchCriteria criteria = new CatalogEntrySearchCriteria();
            criteria.RecordsToRetrieve = 200; // there is a default of 50

            //criteria.Locale = "en"; // Have to be there... else no hits (differ from the sdk)
            criteria.Locale = ContentLanguage.PreferredCulture.TwoLetterISOLanguageName;
            criteria.SearchPhrase = "shirt";
            criteria.SearchPhrase = keyWord;

            // Add more to the criteria
            criteria.CatalogNames.Add("Fashion"); // ...if we know what catalog to search in, not mandatory
            //criteria.IgnoreFilterOnLanguage = true; // how does this work... need the locale anyway

            criteria.ClassTypes.Add(EntryType.Variation);
            criteria.MarketId = "DEFAULT";
            criteria.IsFuzzySearch = true;
            criteria.FuzzyMinSimilarity = 0.7F;
            criteria.IncludeInactive = true;

            // the _outline field
            System.Collections.Specialized.StringCollection sc =
                new System.Collections.Specialized.StringCollection();
            sc.Add("Fashion/Clothes_1/Men_1/Shirts_1"); // 
            sc.Add("Fashion/Clothes_1/UniSex_1"); //

            criteria.Outlines = sc; // another "AND"

            // Add facets to the criteria... 
            // As below, we add to the "used filters"

            Mediachase.Search.SimpleValue svWhite = new SimpleValue();
            svWhite.value = "white";
            svWhite.key = "white";
            criteria.Add("color", svWhite);

            //  
            Mediachase.Search.SimpleValue svBlue = new SimpleValue();
            svBlue.value = "blue";
            svBlue.key = "blue";
            criteria.Add("color", svBlue);


            Mediachase.Search.SimpleValue svVolvo = new SimpleValue();
            svVolvo.value = "volvo";
            svVolvo.key = "volvo";
            criteria.Add("brand", svVolvo);

            Mediachase.Search.SimpleValue svSaab = new SimpleValue();
            svSaab.value = "saab";
            svSaab.key = "saab";
            criteria.Add("brand", svSaab);

            // ...get a FacetGroups "in there"
            SearchFilter configFilter = new SearchFilter();

            configFilter.Values = new SearchFilterValues();
            configFilter.Values.SimpleValue = new SimpleValue[] { svWhite, svBlue, svVolvo, svSaab };

            // use the manager for search and for index management
            //SearchManager manager = new SearchManager("CommerceTraining");////
            SearchManager manager = new SearchManager("ECApplication");



            #region Filters

            // Filters from the XML file 
            string _SearchConfigPath = //"~/Configs/Mediachase.Search.Filters.config";
            @"C:\Episerver6\CommerceTraining\CommerceTraining\Configs\Mediachase.Search.Filters.config";
            TextReader reader = new StreamReader(_SearchConfigPath);
            XmlSerializer serializer = new XmlSerializer((typeof(SearchConfig)));
            _SearchConfig = (SearchConfig)serializer.Deserialize(reader);
            reader.Close();

            foreach (SearchFilter filter in _SearchConfig.SearchFilters)
            {
                criteria.Add(filter);
            }

            #endregion

            // Do search
            ISearchResults results = manager.Search(criteria);

            // ...different return types - same method
            //SearchFilterHelper.Current.SearchEntries()

            ISearchFacetGroup[] facets = results.FacetGroups;

            int[] ints = results.GetKeyFieldValues<int>();

            // CMS style (better)... using ReferenceConverter and ContentLoader 
            List<ContentReference> refs = new List<ContentReference>();
            ints.ToList().ForEach(i => refs.Add(_referenceConverter.GetContentLink(i, CatalogContentType.CatalogEntry, 0)));

            localContent = _contentLoader.GetItems(refs, new LoaderOptions()); // use this in CMS 8+

            // ToDo: Facets
            List<string> facetList = new List<string>();

            int facetGroups = results.FacetGroups.Count();

            foreach (ISearchFacetGroup item in results.FacetGroups)
            {
                foreach (var item2 in item.Facets)
                {
                    facetList.Add(String.Format("{0} {1} ({2})", item.Name, item2.Name, item2.Count));
                }
            }

            var searchResultViewModel = new SearchResultViewModel();

            searchResultViewModel.totalHits = new List<string> { "" }; // change
            searchResultViewModel.nodes = localContent.OfType<FashionNode>();
            searchResultViewModel.products = localContent.OfType<ShirtProduct>();
            searchResultViewModel.variants = localContent.OfType<ShirtVariation>();
            searchResultViewModel.allContent = localContent;
            searchResultViewModel.facets = facetList;


            return View(searchResultViewModel);

        }

        #endregion

    }
}
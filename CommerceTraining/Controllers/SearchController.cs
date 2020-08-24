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
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce;

namespace CommerceTraining.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        protected IEnumerable<IContent> localContent { get; set; }
        protected readonly IContentLoader _contentLoader;
        protected readonly ReferenceConverter _referenceConverter;
        protected readonly UrlResolver _urlResolver;
        protected readonly ICatalogSystem _catalogSystem;

        public SearchController(IContentLoader contentLoader
            , ReferenceConverter referenceConverter
            , UrlResolver urlResolver
            , ICatalogSystem catalogSystem)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
            _catalogSystem = catalogSystem;
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

            // CMS style (better)... using ReferenceConverter and ContentLoader 
            List<ContentReference> refs = new List<ContentReference>();
            ints.ToList().ForEach(i => refs.Add(_referenceConverter.GetContentLink(i, CatalogContentType.CatalogEntry, 0)));

            // LoaderOptions() is new in CMS 8
            // ILanguageSelector selector = ServiceLocator.Current.GetInstance<ILanguageSelector>(); // obsolete
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
            //Qs.temp();

            return null; // ...for now
        }

        public ActionResult FindUnified(string keyWord)
        {
            var model = new SearchResultViewModel
            {
                SearchQuery = keyWord,
                //Results = SearchClient.Instance.UnifiedSearchFor(keyWord).GetResult(),
                Results = SearchClient.Instance.UnifiedSearchFor(keyWord).ApplyBestBets().GetResult()
            };

            return View(model);
        }

        public ActionResult FindExtras(string keyWord)
        {
            IClient client = SearchClient.Instance;
            FindQueries Qs = new FindQueries(client);
            
            // ContentReferences for demoing
            ContentReference theVRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            ContentReference thePRef = _referenceConverter.GetContentLink("Long-Sleeve_Shirt_1");
            ContentReference thePackageRef = _referenceConverter.GetContentLink("SomePackage_1");
            ContentReference theBundleRef = _referenceConverter.GetContentLink("SomeBundle_1");
            ContentReference theNRef = _referenceConverter.GetContentLink("Men_1");

            // Pricing - Inventory
            Qs.SDKExamples(theVRef);

            Qs.VariationExamples(theVRef);

            Qs.ProductExamples(thePRef);

            Qs.GetEntriesByMarket(MarketId.Default);

            Qs.NodeExamples(theNRef);

            Qs.BundleExamples(theBundleRef);

            Qs.PackageExamples(thePackageRef);

            return null;
        }

        #region ProviderModelQueries 
        Injected<LanguageResolver> _langResolver;

        private static volatile SearchConfig _SearchConfig = null;
        public ActionResult ProviderModelQuery(string keyWord)
        {
            // Create criteria
            CatalogEntrySearchCriteria criteria = new CatalogEntrySearchCriteria
            {
                RecordsToRetrieve = 200, // there is a default of 50
                // Locale have to be there... else no hits 
                Locale = ContentLanguage.PreferredCulture.TwoLetterISOLanguageName,
                SearchPhrase = keyWord
            };

            // Add more to the criteria
            criteria.Sort = CatalogEntrySearchCriteria.DefaultSortOrder;
            criteria.CatalogNames.Add("Fashion"); // ...if we know what catalog to search in, not mandatory
            //criteria.IgnoreFilterOnLanguage = true; // if we want to search all languages... need the locale anyway

            criteria.ClassTypes.Add(EntryType.Variation);
            criteria.MarketId = MarketId.Default; // should use the ICurrentMarket service, of course...

            criteria.IsFuzzySearch = true;
            criteria.FuzzyMinSimilarity = 0.7F;

            criteria.IncludeInactive = true;

            // the _outline field
            System.Collections.Specialized.StringCollection sc =
                new System.Collections.Specialized.StringCollection
                {
                    "Fashion/Clothes_1/Men_1/Shirts_1",
                    "Fashion/Clothes_1/UniSex_1"
                };
            criteria.Outlines = sc; // another "AND"

            #region SimpleWalues
            ///*
            // Add facets to the criteria... and later prepare them for the "search result" as FacetGroups
            // With the below only these values are in the result... no Red or RollsRoys
            Mediachase.Search.SimpleValue svWhite = new SimpleValue
            {
                value = "white",
                key = "white",
                locale = "en",
                Descriptions = new Descriptions { defaultLocale = "en" }
            };
            var descWhite = new Description
            {
                locale = "en",
                Value = "White"
            };
            svWhite.Descriptions.Description = new[] { descWhite };

            // If added like this it ends up in "ActiveFields" of the criteria and the result is filtered
            //criteria.Add("color", svWhite);
            // ...also the facetGroups on the "result" are influenced

            Mediachase.Search.SimpleValue svBlue = new SimpleValue
            {
                value = "blue",
                key = "blue",
                locale = "en",
                Descriptions = new Descriptions { defaultLocale = "en" }
            };
            var descBlue = new Description
            {
                locale = "en",
                Value = "Blue"
            };
            svBlue.Descriptions.Description = new[] { descBlue };
            //criteria.Add("color", svBlue);

            Mediachase.Search.SimpleValue svVolvo = new SimpleValue
            {
                value = "volvo",
                key = "volvo",
                locale = "en",
                Descriptions = new Descriptions { defaultLocale = "en" }
            };
            var descVolvo = new Description
            {
                locale = "en",
                Value = "volvo"
            };
            svVolvo.Descriptions.Description = new[] { descVolvo };
            //criteria.Add("brand", svVolvo);

            Mediachase.Search.SimpleValue svSaab = new SimpleValue
            {
                value = "saab",
                key = "saab",
                locale = "en",
                Descriptions = new Descriptions { defaultLocale = "en" }
            };
            var descSaab = new Description
            {
                locale = "en",
                Value = "saab"
            };
            svSaab.Descriptions.Description = new[] { descSaab };
            //criteria.Add("brand", svSaab); 

            #region Debug

            // the above filters the result so only saab (the blue) is there
            // With the above only we see only the Blue shirt... is that a saab - yes
            // New: no xml --> gives one Active and an empty filter even searchFilter.Values.SimpleValue below is there
            // New: outcommenting the above line --> and add XML file ... no "Active Fileds"
            // Have the Filters added - but no actice fields
            // New: trying this... Brand gets "ActiveField" with volvo & saab.. but the result shows all brands
            // New: outcommenting the below line and adding above only one, the saab
            //criteria.Add("brand", new List<ISearchFilterValue> { svSaab, svVolvo });
            // ...get a FacetGroups "in there"... like with the XML-file ... a manual way to add...
            // ...stuff that is not in the XML-file, or skip the XML File
            // New: taking out the single saab filter added

            #endregion

            SearchFilter searchFilterColor = new SearchFilter
            {
                //field = BaseCatalogIndexBuilder.FieldConstants.Catalog, // Have a bunch
                field = "color",

                // mandatory 
                Descriptions = new Descriptions
                {
                    // another way of getting the language
                    defaultLocale = _langResolver.Service.GetPreferredCulture().Name
                },

                Values = new SearchFilterValues(),
            };

            SearchFilter searchFilterBrand = new SearchFilter
            {
                field = "brand",

                Descriptions = new Descriptions
                {
                    defaultLocale = _langResolver.Service.GetPreferredCulture().Name
                },

                Values = new SearchFilterValues(),
            };

            var descriptionColor = new Description
            {
                locale = "en",
                Value = "Color"
            };

            var descriptionBrand = new Description
            {
                locale = "en",
                Value = "Brand"
            };


            searchFilterColor.Descriptions.Description = new[] { descriptionColor };
            searchFilterBrand.Descriptions.Description = new[] { descriptionBrand };

            searchFilterColor.Values.SimpleValue = new SimpleValue[] { svWhite, svBlue };
            searchFilterBrand.Values.SimpleValue = new SimpleValue[] { svVolvo, svSaab };

            // can do like the below or us the loop further down...
            // the "foreach (SearchFilter item in _NewSearchConfig.SearchFilters)"
            // use these in the second part of the demo... "without XML" ... saw that with XML-style
            criteria.Add(searchFilterColor);
            criteria.Add(searchFilterBrand);

            #region Debug

            // gets the "filters" without this below and the XML... further checks... 
            // do we need this? ... seems not... or it doesn't work like this
            // New: Have XML and commenting out the below lines Looks the same as with it
            // New: second...outcommenting the criteria.Add(searchFilter);
            // the Facets prop is empty......without the XML
            // the below line seems not to work
            // New: adding these again together with the saab above active
            //... difference is the "VariationFilter" 
            // We get the Facets on the criteria, but no facets in the "result" without the XML

            //criteria.Filters = searchFilter; // read-only

            // Without the XML...

            // boom... on a missing "key"... the description
            // when commenting out the criteria.Add() for the simple values...??
            // When adding more to the SearchFilter it works...
            // ... the Simple values are there in the only instance if the filter
            // commenting out and check with the XML
            // when using the XML the groups sit in FacetGroups
            // when using the above... no facet-groups added

            // The same facets added a second time, Filter number 2 and no facet-groups
            //SearchConfig sConf = new SearchConfig();

            #endregion Debug

            //*/
            #endregion SimpleValues

            // use the manager for search and for index management
            SearchManager manager = new SearchManager("ECApplication");

            #region Facets/Filters

            // Filters from the XML file, populates the FacetGroups on the Search result
            string _SearchConfigPath =
            @"C:\Episerver612\CommerceTraining\CommerceTraining\Configs\Mediachase.Search.Filters.config";

            TextReader reader = new StreamReader(_SearchConfigPath);
            XmlSerializer serializer = new XmlSerializer((typeof(SearchConfig)));
            _SearchConfig = (SearchConfig)serializer.Deserialize(reader);
            reader.Close();

            foreach (SearchFilter filter in _SearchConfig.SearchFilters)
            {
                // Step 1 - use the XML file
                //criteria.Add(filter); 
            }

            // Manual...
            SearchConfig _NewSearchConfig = new SearchConfig
            {
                SearchFilters = new SearchFilter[] { searchFilterColor, searchFilterBrand }
            };

            // can do like this, but there is another way (a bit above)
            foreach (SearchFilter item in _NewSearchConfig.SearchFilters)
            {
                // Step 2 - skip the XML file
                //criteria.Add(item); 
            }

            #endregion

            // Do search
            ISearchResults results = manager.Search(criteria);

            #region Debug


            // doens't work
            //FacetGroup facetGroup = new FacetGroup("Bogus", "Bummer");
            //results.FacetGroups = new[] { facetGroup };

            // ...different return types - same method
            //SearchFilterHelper.Current.SearchEntries()

            // out comment and do a new try
            //ISearchFacetGroup[] facets = results.FacetGroups;

            // NEW: adding these ... for the provider, last line doesn's assign
            //ISearchFacetGroup[] searchFacetGroup0 = new SearchFacetGroup() { };
            FacetGroup facetGroup0 = new FacetGroup("colorgroup", "dummy"); //{ "",""}; // FacetGroup("brand","volvo");
            Facet f1 = new Facet(facetGroup0, svWhite.key, svWhite.value, 1);
            facetGroup0.Facets.Add(f1);
            //facets[1] = facetGroup0;
            ISearchFacetGroup[] searchFacetGroup = new FacetGroup[] { facetGroup0 };
            //searchFacetGroup.Facets.Add(f1);
            //results.FacetGroups = searchFacetGroup; // nothing happens here, facet-group still empty

            #endregion

            int[] ints = results.GetKeyFieldValues<int>();

            // The DTO-way
            CatalogEntryDto dto = _catalogSystem.GetCatalogEntriesDto(ints);

            // CMS style (better)... using ReferenceConverter and ContentLoader 
            List<ContentReference> refs = new List<ContentReference>();
            ints.ToList().ForEach(i => refs.Add(_referenceConverter.GetContentLink(i, CatalogContentType.CatalogEntry, 0)));

            localContent = _contentLoader.GetItems(refs, new LoaderOptions()); // 

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

            var searchResultViewModel = new SearchResultViewModel
            {
                totalHits = new List<string> { "" }, // change
                nodes = localContent.OfType<FashionNode>(),
                products = localContent.OfType<ShirtProduct>(),
                variants = localContent.OfType<ShirtVariation>(),
                allContent = localContent,
                facets = facetList
            };


            return View(searchResultViewModel);

        }

        #endregion

    }
}
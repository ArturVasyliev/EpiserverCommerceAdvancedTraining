using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace CommerceTraining.Controllers
{
    public class SearchDemoController : Controller
    {
        // GET: SearchDemo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProviderModelQuery(string keyWord)
        {
            var vmodel = new PMSearchResultViewModel();
            vmodel.SearchQueryText = keyWord;
            
            // Create criteria
            CatalogEntrySearchCriteria criteria = new CatalogEntrySearchCriteria
            {
                RecordsToRetrieve = 200, // there is a default of 50
                // Locale have to be there... else no hits 
                Locale = ContentLanguage.PreferredCulture.TwoLetterISOLanguageName,
                SearchPhrase = keyWord
            };

            #region Options
            //criteria.Sort = CatalogEntrySearchCriteria.DefaultSortOrder;
            //criteria.CatalogNames.Add("Fashion");

            //criteria.ClassTypes.Add(EntryType.Variation);
            //criteria.MarketId = MarketId.Default;

            //criteria.IsFuzzySearch = true;
            //criteria.FuzzyMinSimilarity = 0.7F;

            //criteria.IncludeInactive = true;

            //System.Collections.Specialized.StringCollection sc =
            //    new System.Collections.Specialized.StringCollection
            //    {
            //        "Fashion/Clothes_1/Men_1/Shirts_1",
            //        "Fashion/Clothes_1/UniSex_1"
            //    };
            //criteria.Outlines = sc;
            #endregion Options

            //string _SearchConfigPath =
            //@"C:\Episerver612\CommerceTraining\CommerceTraining\Configs\Mediachase.Search.Filters.config";

            //TextReader reader = new StreamReader(_SearchConfigPath);
            //XmlSerializer serializer = new XmlSerializer((typeof(SearchConfig)));
            //var _SearchConfig = (SearchConfig)serializer.Deserialize(reader);
            //reader.Close();

            //foreach (SearchFilter filter in _SearchConfig.SearchFilters)
            //{
            //    criteria.Add(filter); 
            //}

            CreateFacetsByCode(criteria);

            // use the manager for search and for index management
            SearchManager manager = new SearchManager("ECApplication");

            // Do search
            ISearchResults results = manager.Search(criteria);

            vmodel.SearchResults = results.Documents.ToList();
            vmodel.FacetGroups = results.FacetGroups.ToList();
            vmodel.ResultCount = results.Documents.Count.ToString();

            return View(vmodel);
        }

        public ActionResult ProviderModelFilteredSearch(string keyWord, string group, string facet)
        {
            var vmodel = new PMSearchResultViewModel();
            vmodel.SearchQueryText = keyWord;
            
            CatalogEntrySearchCriteria criteria = new CatalogEntrySearchCriteria
            { 
                Locale = ContentLanguage.PreferredCulture.TwoLetterISOLanguageName,
                SearchPhrase = keyWord
            };

            //string _SearchConfigPath =
            //@"C:\Episerver612\CommerceTraining\CommerceTraining\Configs\Mediachase.Search.Filters.config";

            //TextReader reader = new StreamReader(_SearchConfigPath);
            //XmlSerializer serializer = new XmlSerializer((typeof(SearchConfig)));
            //var _SearchConfig = (SearchConfig)serializer.Deserialize(reader);
            //reader.Close();

            //foreach (SearchFilter filter in _SearchConfig.SearchFilters)
            //{
            //    // Step 1 - use the XML file
            //    criteria.Add(filter);
            //}

            CreateFacetsByCode(criteria);
            
            foreach (SearchFilter filter in criteria.Filters)
            {
                if(filter.field.ToLower() == group.ToLower())
                {
                    var svFilter = filter.Values.SimpleValue
                        .FirstOrDefault(x => x.value.Equals(facet, StringComparison.OrdinalIgnoreCase));
                    if (svFilter != null)
                    {
                        //This overload to Add causes the filter to be applied
                        criteria.Add(filter.field, svFilter);
                    }
                }
            }

            // use the manager for search and for index management
            SearchManager manager = new SearchManager("ECApplication");

            // Do search
            ISearchResults results = manager.Search(criteria);

            vmodel.SearchResults = results.Documents.ToList();
            vmodel.FacetGroups = results.FacetGroups.ToList();
            vmodel.ResultCount = results.Documents.Count.ToString();

            return View("ProviderModelQuery", vmodel);
        }

        private void CreateFacetsByCode(CatalogEntrySearchCriteria criteria)
        {
            #region Simple Values to be added to filters
            SimpleValue svWhite = new SimpleValue
            {
                value = "white",
                key = "white",
                locale = "en",
                Descriptions = new Descriptions
                {
                    defaultLocale = "en",
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "White" }
                    }
                }
            };

            SimpleValue svBlue = new SimpleValue
            {
                value = "blue",
                key = "blue",
                locale = "en",
                Descriptions = new Descriptions
                {
                    defaultLocale = "en",
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Blue" }
                    }
                }
            };

            SimpleValue svRed = new SimpleValue
            {
                value = "red",
                key = "red",
                locale = "en",
                Descriptions = new Descriptions
                {
                    defaultLocale = "en",
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Red" }
                    }
                }
            };

            SimpleValue svVolvo = new SimpleValue
            {
                value = "volvo",
                key = "volvo",
                locale = "en",
                Descriptions = new Descriptions
                {
                    defaultLocale = "en",
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Volvo" }
                    }
                }
            };

            SimpleValue svSaab = new SimpleValue
            {
                value = "saab",
                key = "saab",
                locale = "en",
                Descriptions = new Descriptions
                {
                    defaultLocale = "en",
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Saab" }
                    }
                }
            };
            #endregion

            #region Search Filters
            var _langResolver = ServiceLocator.Current.GetInstance<LanguageResolver>();

            SearchFilter searchFilterColor = new SearchFilter
            {
                field = "color",

                // mandatory 
                Descriptions = new Descriptions
                {
                    defaultLocale = _langResolver.GetPreferredCulture().Name,
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Color" }
                    }
                },

                Values = new SearchFilterValues
                {
                    SimpleValue = new SimpleValue[]
                    {
                        svWhite,
                        svBlue,
                        svRed
                    }
                }
            };

            SearchFilter searchFilterBrand = new SearchFilter
            {
                field = "brand",

                Descriptions = new Descriptions
                {
                    defaultLocale = _langResolver.GetPreferredCulture().Name,
                    Description = new Description[]
                    {
                        new Description { locale = "en", Value = "Brand" }
                    }
                },

                Values = new SearchFilterValues
                {
                    SimpleValue = new SimpleValue[]
                    {
                        svSaab,
                        svVolvo
                    }
                }
            };
            #endregion

            criteria.Add(searchFilterColor);
            criteria.Add(searchFilterBrand);
        }
    }
}
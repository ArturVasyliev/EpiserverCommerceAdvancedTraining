using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Commerce;
using EPiServer.Find.Framework;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class FindSearchDemoController : Controller
    {
        // GET: FindSearchDemo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FindQueryIntegrated(string keyWord)
        {
            var viewModel = new FindResultViewModel
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;


            var result = client.Search<ShirtVariation>()
                .For(keyWord)
                //.Filter(x => x.InStockQuantityLessThan(100))
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .TermsFacetFor(x => x.Color)
                .GetContentResult();

            viewModel.ColorFacets = result.TermsFacetFor(x => x.Color).Terms.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();
            viewModel.ShirtVariants = result.ToList();

            return View(viewModel);
        }

        public ActionResult FacetFilteredSearch(string keyWord, string facet)
        {
            var viewModel = new FindResultViewModel
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;

            var result = client.Search<ShirtVariation>()
                .For(keyWord)
                .Filter(x => x.Color.Match(facet))
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .TermsFacetFor(x => x.Color)
                .GetContentResult();

            viewModel.ColorFacets = result.TermsFacetFor(x => x.Color).Terms.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();
            viewModel.ShirtVariants = result.ToList();

            return View("FindQueryIntegrated", viewModel);
        }

        public ActionResult ProjectionResult(string keyWord)
        {
            var viewModel = new ProjectedShirtViewModel()
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;

            var result = client.Search<ShirtVariation>()
                .For(keyWord)
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .Select(x => new ProjectedShirt
                {
                    Name = x.Name,
                    Color = x.Color,
                    Brand = x.Brand,
                    UrlLink = x.ContentLink
                })
                .GetResult();

            viewModel.Shirts = result.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();

            return View(viewModel);
        }
    }
}
//
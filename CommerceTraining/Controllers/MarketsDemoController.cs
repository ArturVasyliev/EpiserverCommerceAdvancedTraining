using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class MarketsDemoController : Controller
    {
        private IMarketService _marketService;
        private ICurrentMarket _currentMarket;
        private ReferenceConverter _referenceConverter;
        private IContentLoader _contentLoader;

        public MarketsDemoController(IMarketService marketService, ICurrentMarket currentMarket, ReferenceConverter referenceConverter, IContentLoader contentLoader)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
        }

        public ActionResult Index()
        {
            var viewModel = new DemoMarketsViewModel();
            viewModel.MarketList = _marketService.GetAllMarkets();
            viewModel.SelectedMarket = _currentMarket.GetCurrentMarket();

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            viewModel.Shirt = _contentLoader.Get<ShirtVariation>(shirtRef);

            return View(viewModel);

        }
        public ActionResult ChangeDefaultMarket(string selectedMarket)
        {
            if (selectedMarket != null)
            {
                _currentMarket.SetCurrentMarket(new MarketId(selectedMarket));
            }
            var viewModel = new DemoMarketsViewModel();
            viewModel.MarketList = _marketService.GetAllMarkets();
            viewModel.SelectedMarket = _currentMarket.GetCurrentMarket();

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            viewModel.Shirt = _contentLoader.Get<ShirtVariation>(shirtRef);

            return View("Index", viewModel);
        }

    }
}
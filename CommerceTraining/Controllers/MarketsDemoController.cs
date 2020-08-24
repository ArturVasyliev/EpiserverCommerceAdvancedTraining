using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
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
        private IOrderRepository _orderRepository;
        private IOrderGroupFactory _orderGroupFactory;
        private ITaxCalculator _taxCalculator;

        public MarketsDemoController(IMarketService marketService, ICurrentMarket currentMarket,
            ReferenceConverter referenceConverter, IContentLoader contentLoader,
            IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory,
            ITaxCalculator taxCalculator)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _taxCalculator = taxCalculator;
        }

        public ActionResult Index()
        {
            var viewModel = new DemoMarketsViewModel();
            viewModel.MarketList = _marketService.GetAllMarkets();
            viewModel.SelectedMarket = _currentMarket.GetCurrentMarket();

            var shirtRef = _referenceConverter.GetContentLink("Long Sleeve Shirt White Small_1");
            viewModel.Shirt = _contentLoader.Get<ShirtVariation>(shirtRef);

            GetTaxInfo(viewModel);

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

            GetTaxInfo(viewModel);

            return View("Index", viewModel);
        }

        private void GetTaxInfo(DemoMarketsViewModel viewModel)
        {
            ICart cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "BogusCart");

            IOrderAddress bogusAddress = _orderGroupFactory.CreateOrderAddress(cart);
            bogusAddress.CountryCode = viewModel.SelectedMarket.Countries.FirstOrDefault();
            bogusAddress.City = "Stockholm";
            //viewModel.TaxAmountOldSchool = GetTaxOldSchool(viewModel, bogusAddress);
        }

        //private Money GetTaxOldSchool(DemoMarketsViewModel viewModel, IOrderAddress orderAddress)
        //{
        //    string taxCategory = CatalogTaxManager.GetTaxCategoryNameById((int)viewModel.Shirt.TaxCategoryId);

        //    viewModel.Taxes = OrderContext.Current.GetTaxes(Guid.Empty,
        //        taxCategory, viewModel.SelectedMarket.DefaultLanguage.TwoLetterISOLanguageName, 
        //        orderAddress);

        //    decimal decTaxTotal = (decimal)(from x in viewModel.Taxes
        //                                    where x.TaxType == TaxType.SalesTax
        //                                    select x).Sum((ITaxValue x) => x.Percentage);

        //    decimal itemPrice = viewModel.Shirt.GetDefaultPrice().UnitPrice;

        //    return new Money(itemPrice * decTaxTotal / 100m, viewModel.SelectedMarket.DefaultCurrency);
        //}

    }
}
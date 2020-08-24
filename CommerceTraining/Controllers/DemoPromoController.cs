using CommerceTraining.Models.ViewModels;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Controllers
{
    public class DemoPromoController : Controller
    {
        private IPromotionEngine _promoEngine;
        private ReferenceConverter _refConverter;
        private ICurrentMarket _currentMarket;

        public DemoPromoController(IPromotionEngine promotionEngine, ReferenceConverter referenceConverter, ICurrentMarket currentMarket)
        {
            _promoEngine = promotionEngine;
            _refConverter = referenceConverter;
            _currentMarket = currentMarket;
        }
        public ActionResult Index()
        {
            var viewModel = new DemoPromoViewModel();

            viewModel.CatalogItems.Add(new CatItem
            {
                Code = "Long Sleeve Shirt White Small_1",
                Quantity = 0
            });
            viewModel.CatalogItems.Add(new CatItem
            {
                Code = "Long-Sleeve-Shirt-Blue-Medium_1",
                Quantity = 0
            });

            return View(viewModel);
        }

        public ActionResult EvalPromos(DemoPromoViewModel viewModel)
        {
            var market = _currentMarket.GetCurrentMarket();
            var inMemOrderGroup = new InMemoryOrderGroup(market, market.DefaultCurrency);

            foreach (var item in viewModel.CatalogItems)
            {
                if (item.Quantity > 0)
                {
                    var inMemLineItem = new InMemoryLineItem
                    {
                        Code = item.Code,
                        Quantity = item.Quantity
                    };
                    inMemOrderGroup.GetFirstShipment().LineItems.Add(inMemLineItem);
                }

            }

            var promoSettings = new PromotionEngineSettings(RequestFulfillmentStatus.All, true);
            viewModel.Rewards = _promoEngine.Run(inMemOrderGroup, promoSettings);

            viewModel.CartItems = inMemOrderGroup.GetFirstShipment().LineItems;
            if(inMemOrderGroup.GetFirstForm().Promotions.Count > 0)
            {
                viewModel.PromoItems = inMemOrderGroup.GetFirstForm().Promotions.First().Entries;
            }       

            return View("Index", viewModel);
        }
    }
}
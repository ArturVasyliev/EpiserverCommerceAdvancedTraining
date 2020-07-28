using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Commerce.Catalog.ContentTypes;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog;
using CommerceTraining.SupportingClasses;
using System;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce;
//using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Controllers
{
    public class NodeController : CatalogControllerBase<FashionNode>
    {
        private readonly PromotionEngine _promoEngine;
        private readonly ICurrentMarket _currMarket;

        // ... ToDo: "into the course"
        public NodeController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver
            , AssetUrlConventions assetUrlConvensions // Adv.
            , ICurrentMarket currentMarket
            , PromotionEngine promoEngine
            )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver, currentMarket)
        {
            _promoEngine = promoEngine;
            _currMarket = currentMarket;
        }

        public ActionResult Index(NodeContent currentContent)
        {
            // could change the name ...it´s a viewModel
            var model = new NodeEntryCombo
            {
                nodes = GetNodes(currentContent.ContentLink),
                entries = GetEntries(currentContent.ContentLink),
                promotionMessage = "Wait and see"
            }; 
            
            return View(model);

        }

        private string GetPromotionMessage(IEnumerable<VariationContent> entries)
        {
            IMarket market = _currMarket.GetCurrentMarket();

            if (entries.Count() >= 1)
            {
                List<ContentReference> contentRefs = new List<ContentReference>();
                foreach (var item in entries)
                {
                    contentRefs.Add(item.ContentLink);
                }
                IEnumerable<RewardDescription> desc;
                
                desc = _promoEngine.Evaluate(contentRefs, market, market.DefaultCurrency, RequestFulfillmentStatus.All);

                string s = String.Empty;
                foreach (var item in desc)
                {
                    s += item.Description;
                }

                var p = _promoEngine.GetDiscountPrices(contentRefs, market, market.DefaultCurrency);
                foreach (var item in p)
                {
                    foreach (var item2 in item.DiscountPrices)
                    {
                        s += item2.Price.ToString("C");
                    }
                }

                return s;
            }
            else
            {
                return "No promotions in this category";
            }
        }


        public void DeadEnd(string id)
        {
            try
            {

            }
            catch (DivideByZeroException e)
            {
                throw new DivideByZeroException(id);
            }
        }
    }
}
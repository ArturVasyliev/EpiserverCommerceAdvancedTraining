using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Commerce.Marketing;
using EPiServer.DataAbstraction;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace CommerceTraining.Controllers
{
    public class PromotionPageController : PageController<PromotionPage>
    {
        List<string> entryList = new List<string>();

        public ActionResult Index(PromotionPage currentPage)
        {
            /* Implementation of action. You can create your own view model class that you pass to the view or
             * you can pass the page type for simpler templates */

            WhatEntriesAreOnPromotion(); // no success, so far

            return View(currentPage);
        }

        Injected<IPromotionEngine> _promoEngine;
        Injected<ContentRootService> _rootService;
        Injected<IContentLoader> _loader;
        Injected<PromotionProcessorResolver> _resolver;
        public void WhatEntriesAreOnPromotion()
        {
            var root = _rootService.Service.Get("SysCampaignRoot");
            var c = _loader.Service.GetChildren<SalesCampaign>(root); 

            // loop through campaigns
            foreach (var item in c)
            {
                // get complaints on "all threads must run"...?
                var p = _promoEngine.Service.GetPromotionItemsForCampaign(
                   //_loader.Service, _resolver.Service,
                    item.ContentLink);
                
                //var e = _promoEngine.Service.
                //var items = p.First().Condition.Items;
                var t = item.GetOriginalType();
                var p1 = _loader.Service.Get<SalesCampaign>(item.ContentLink);
                
                if (p.GetOriginalType() == typeof(EntryPromotion))
                {
                    foreach (var item2 in p)
                    {
                        var i = item2.Condition.Items;
                        foreach (var item3 in i)
                        {
                            if (item3.GetOriginalType() == typeof(EntryContentBase))
                            {
                                var entry = _loader.Service.Get<EntryContentBase>(item3);
                                var entryName = entry.Name;
                                entryList.Add(entryName);
                            }

                            // what about nodes, shipping and cart-promos ...
                        }

                        //var pp = item2.Promotion;
                        //var ppp = pp.GetOriginalType();
                        //if (pp.DiscountType == DiscountType.LineItem)
                        //{


                        //}
                    }

                }
            }
        }
    }
}
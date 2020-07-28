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

            return View(currentPage);
        }
                
    }
}
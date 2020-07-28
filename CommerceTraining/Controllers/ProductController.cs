using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using CommerceTraining.Models.Pages;
using EPiServer.Web.Routing;
using EPiServer.Commerce.Catalog;

// new in 9.1
using EPiServer.Find.Commerce;
using CommerceTraining.SupportingClasses; // extensions are here

namespace CommerceTraining.Controllers
{

    // have a try and start from the "ASimple" and use the pattern here
    // ...doing it basic --> no ViewModel and other luxuries
    public class ProductController : ContentController<ShirtProduct>
    {
        
        public ActionResult Index(ShirtProduct currentContent)
        {
            return View(currentContent);
        }
    }
}
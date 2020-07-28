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
using CommerceTraining.SupportingClasses;

namespace CommerceTraining.Controllers
{
    public class AccessoryNodeController : ContentController<AccessoryNode>
    {

        public EntryContentBase myChildren { get; set; }

        public ActionResult Index(AccessoryNode currentContent)
        {
            return View(currentContent);
        }


        public IEnumerable<EntryContentBase> getThem(ContentReference link)
        {
            return ServiceLocator.Current.GetInstance<IContentLoader>().GetChildren<EntryContentBase>(link);
        }
    }
}
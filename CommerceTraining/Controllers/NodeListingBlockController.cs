using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Blocks;
using EPiServer.ServiceLocation;
using CommerceTraining.Models.ViewModels;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace CommerceTraining.Controllers
{
    public class NodeListingBlockController : BlockController<NodeListingBlock>
    {

        Injected<IContentLoader> _loader;
        ContentReference cref { get; set; }

        public override ActionResult Index(NodeListingBlock currentBlock)
        {

            NodeListingBlockViewModel model = new NodeListingBlockViewModel();
            model.Name = currentBlock.Name;
            model.entries = GetEntries(currentBlock);

            return PartialView(model);
        }

        private IEnumerable<EPiServer.Commerce.Catalog.ContentTypes.EntryContentBase> GetEntries(NodeListingBlock currentBlock)
        {
            ContentReference cRef = ContentReference.StartPage;
            StartPage homePage = _loader.Service.Get<StartPage>(cRef);
            IEnumerable<EntryContentBase> children = _loader.Service.GetChildren<EntryContentBase>(homePage.Settings.WeeklySpecials);

            // could get associations instead
            return children;
        }
    }
}

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
using Mediachase.Commerce.Catalog;
using EPiServer.ServiceLocation;
using EPiServer.Commerce.Catalog.ContentTypes;
using CommerceTraining.Models.ViewModels;

namespace CommerceTraining.Controllers
{
    public class ProductBlockController : BlockController<ProductBlock>
    {
        Injected<ReferenceConverter> _refConv;
        Injected<IContentLoader> _loader;
        public override ActionResult Index(ProductBlock currentBlock)
        {

            var theSKU = _loader.Service.Get<EntryContentBase>(currentBlock.ProductReference);

            var model = new ProductBlockViewModel
            {
                NameAndMore = currentBlock.ProductName + " something else " + theSKU.Name
            };
            

            return PartialView(model);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using EPiServer.ServiceLocation;
using CommerceTraining.Models.ViewModels;

namespace CommerceTraining.Controllers
{
    public class CatalogRoutingStartController : PageController<CatalogRoutingStartPage>
    {
        public ActionResult Index(CatalogRoutingStartPage currentPage)
        {
            /* Implementation of action. You can create your own view model class that you pass to the view or
             * you can pass the page type for simpler templates */
            
            return View(currentPage);
        }

        public IEnumerable<ContentReference> AllCategories()
        {
            List<ContentReference> localList = new List<ContentReference>();
            List<int> nodeIds = new List<int>();

            CatalogEntryDto dto = CatalogContext.Current.GetCatalogEntriesDto("Fashion");

            CatalogRelationDto relDto = CatalogContext.Current.GetCatalogRelationDto(1, 0, 0, null,
                new CatalogRelationResponseGroup(CatalogRelationResponseGroup.ResponseGroup.NodeEntry));


            foreach (CatalogRelationDto.NodeEntryRelationRow item in relDto.NodeEntryRelation)
            {
                if (!nodeIds.Contains(item.CatalogNodeId))
                {
                    nodeIds.Add(item.CatalogNodeId);
                }
            }

            ReferenceConverter refConv = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            foreach (int item in nodeIds)
            {
                localList.Add(refConv.GetContentLink(item, CatalogContentType.CatalogNode, 0));
            }
            
            return localList;
        }

    }
}
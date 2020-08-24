using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceManager
{
    public class IndexMoreProperties : BaseCatalogIndexBuilder
    {
        private ICatalogSystem _catalog;
        protected IndexMoreProperties(ICatalogSystem catalogSystem, IPriceService priceService, IInventoryService inventoryService, MetaDataContext metaDataContext, CatalogItemChangeManager catalogItemChangeManager, NodeIdentityResolver nodeIdentityResolver) : base(catalogSystem, priceService, inventoryService, metaDataContext, catalogItemChangeManager, nodeIdentityResolver)
        {
            _catalog = catalogSystem;
        }

        public IndexMoreProperties() : this(ServiceLocator.Current.GetInstance<ICatalogSystem>(),
            ServiceLocator.Current.GetInstance<IPriceService>(),
            ServiceLocator.Current.GetInstance<IInventoryService>(),
            ServiceLocator.Current.GetInstance<MetaDataContext>(),
            ServiceLocator.Current.GetInstance<CatalogItemChangeManager>(),
            ServiceLocator.Current.GetInstance<NodeIdentityResolver>()
            )
        { }

        protected override void OnCatalogEntryIndex(ref SearchDocument document, CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            base.OnCatalogEntryIndex(ref document, entry, language);
        }
    }
}
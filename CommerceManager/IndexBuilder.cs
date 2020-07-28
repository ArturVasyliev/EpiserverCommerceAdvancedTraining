using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Loggers;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
using Mediachase.Search.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceManager
{
    public class IndexBuilder : BaseCatalogIndexBuilder
    {
        private ICatalogSystem _catalog;
        public IndexBuilder() : this(
            ServiceLocator.Current.GetInstance<ICatalogSystem>(),
            ServiceLocator.Current.GetInstance<IPriceService>(),
            ServiceLocator.Current.GetInstance<IInventoryService>(),
            ServiceLocator.Current.GetInstance<MetaDataContext>(),
            ServiceLocator.Current.GetInstance<CatalogItemChangeManager>(),
            ServiceLocator.Current.GetInstance<NodeIdentityResolver>()
            )
        { }

        public IndexBuilder(ICatalogSystem catalogSystem,
            IPriceService priceService,
            IInventoryService inventoryService,
            MetaDataContext metaDataContext,
            CatalogItemChangeManager catalogItemChangeManager,
            NodeIdentityResolver nodeIdentityResolver) : base(catalogSystem, priceService, inventoryService, metaDataContext, catalogItemChangeManager, nodeIdentityResolver)
        {
            _catalog = catalogSystem;
        }
        protected override void OnCatalogEntryIndex(ref SearchDocument document, Mediachase.Commerce.Catalog.Dto.CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            if (entry != null && entry.ClassTypeId.Equals(EntryType.Product, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!MetaDataContextClone.Language.Equals(language, StringComparison.InvariantCultureIgnoreCase))
                {
                    MetaDataContextClone.Language = language;
                }

                CatalogRelationDto relationDto = _catalog.GetCatalogRelationDto(0, 0, entry.CatalogEntryId, string.Empty,
                    new CatalogRelationResponseGroup(CatalogRelationResponseGroup.ResponseGroup.CatalogEntry));

                if (relationDto != null && relationDto.CatalogEntryRelation != null && relationDto.CatalogEntryRelation.Count > 0)
                {
                    List<int> childIds = new List<int>();

                    foreach (CatalogRelationDto.CatalogEntryRelationRow relationRow in relationDto.CatalogEntryRelation)
                    {
                        childIds.Add(relationRow.ChildEntryId);
                    }

                    CatalogEntryDto skuDto = _catalog.GetCatalogEntriesDto(childIds.ToArray(),
                        new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo));

                    List<string> searchProperties = new List<string>
                    {
                        SearchField.Store.YES,
                        SearchField.Index.TOKENIZED,
                        SearchField.IncludeInDefaultSearch.YES
                    };

                    List<string> colorVariations = new List<string>();
                    if (skuDto != null && skuDto.CatalogEntry != null && skuDto.CatalogEntry.Count > 0)
                    {
                        foreach (CatalogEntryDto.CatalogEntryRow row in skuDto.CatalogEntry)
                        {
                            Hashtable hash = ObjectHelper.GetMetaFieldValues(row);
                            if (hash.Contains("Color"))
                            {
                                string color = hash["Color"].ToString();
                                if (!string.IsNullOrEmpty(color) && !colorVariations.Contains(color.ToLower()))
                                {
                                    colorVariations.Add(color.ToLower());
                                    document.Add(new SearchField("Color", color.ToLower(), searchProperties.ToArray()));
                                    OnSearchIndexMessage(new Mediachase.Search.SearchIndexEventArgs(
                                        $"The color {color} was added to the index for {entry.Name}.", 1));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Find.ClientConventions;
using EPiServer.Find.Commerce;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure
{
[ServiceConfiguration(ServiceType = typeof(CatalogContentClientConventions))]
    public class FindCatalogConventions : CatalogContentClientConventions
    {
        //protected override void ApplyIStockPlacementConventions
        //    (TypeConventionBuilder<IStockPlacement> conventionBuilder)
        //{
        //    base.ApplyIStockPlacementConventions(conventionBuilder);
        //    conventionBuilder.ExcludeField(x => x.Inventories());
        //}
    }
}
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace CommerceTraining.SupportingClasses
{
    // didn't work, I don't get it
    public static class ProjectExtensions
    {
        public static IEnumerable<EntryContentBase> GetTheEntries(this NodeContent node)
        {
            return ServiceLocator.Current.GetInstance<IContentLoader>().GetChildren<EntryContentBase>(node.ContentLink);
        }

    }

    //[CatalogContentType(GUID = "94db1554-150d-4334-b540-279320ba34ec"
    //    , DisplayName = "Package"
    //    , AvailableInEditMode = false)
    //    , AvailableContentTypes(Exclude = new Type[] {
    //        typeof(BundleContent), typeof(PackageContent)
    //        , typeof(NodeContentBase) })
    //    ]
    //public class PackageContent : EntryContentBase
    //    , IPricing, IDimensionalStockPlacement, IStockPlacement
    //{
    //    public int? TaxCategoryId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public ContentReference PriceReference { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public ShippingDimensions ShippingDimensions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public decimal? MaxQuantity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public decimal? MinQuantity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public bool TrackInventory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public double Weight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public int? ShippingPackageId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //    public ContentReference InventoryReference { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    public override string ClassTypeId => throw new NotImplementedException();
    //}

}
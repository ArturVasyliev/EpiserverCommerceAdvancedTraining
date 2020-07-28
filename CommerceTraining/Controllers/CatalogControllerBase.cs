using CommerceTraining.Infrastructure;
using CommerceTraining.Models.Media;
using CommerceTraining.Models.Pages;
using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog; // AssetUrlResolver
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Security;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mediachase.Commerce.Security;


namespace CommerceTraining.Controllers
{
    public class CatalogControllerBase<T> : ContentController<T> where T : CatalogContentBase
    {
        public readonly IContentLoader _contentLoader;
        public readonly UrlResolver _urlResolver;
        public readonly AssetUrlResolver _assetUrlResolver;
        public readonly ThumbnailUrlResolver _thumbnailUrlResolver;
        public ICurrentMarket _currentMarket;

        public CatalogControllerBase(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver
            , ICurrentMarket currentMarket
            )
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _assetUrlResolver = assetUrlResolver;
            _thumbnailUrlResolver = thumbnailUrlResolver;
            _currentMarket = currentMarket;
        }

        public string GetDefaultAsset(IAssetContainer contentInstance) 
        {
            return _assetUrlResolver.GetAssetUrl(contentInstance);
        }

        public string GetNamedAsset(IAssetContainer contentInstance, string propName)
        {
            return _thumbnailUrlResolver.GetThumbnailUrl(contentInstance, propName);
        }
        
        public string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

        public List<NameAndUrls> GetNodes(ContentReference contentReference)
        {
            // FilterForVisitor is nice
            IEnumerable<IContent> things = FilterForVisitor.Filter
                (_contentLoader.GetChildren<NodeContent>(
                contentReference, new LoaderOptions()).OfType<NodeContent>());

            List<NameAndUrls> comboList = new List<NameAndUrls>(); // ...to send back to the view

            foreach (NodeContent item in things)
            {
                NameAndUrls comboListitem = new NameAndUrls();
                comboListitem.name = item.Name;
                comboListitem.url = GetUrl(item.ContentLink);

                // Get from default group, "named" in Adv.
                comboListitem.imageUrl = GetDefaultAsset(item);
                comboListitem.imageTumbUrl = GetNamedAsset(item, "Thumbnail");

                comboList.Add(comboListitem);
            }
            return comboList;
        }

        public List<NameAndUrls> GetEntries(ContentReference contentReference)
        {
            // IContent
            IEnumerable<IContent> things = FilterForVisitor.Filter
                (_contentLoader.GetChildren<EntryContentBase>(
                contentReference, new LoaderOptions()));

            List<NameAndUrls> comboList = new List<NameAndUrls>();

            //foreach (var item in entries)
            foreach (EntryContentBase item in things)
            {
                NameAndUrls listItems = new NameAndUrls();
                listItems.name = item.Name;
                listItems.url = GetUrl(item.ContentLink);
                listItems.imageUrl = GetDefaultAsset(item);
                listItems.imageTumbUrl = GetNamedAsset(item, "Thumbnail");

                comboList.Add(listItems);
            }

            return comboList;
        }

        // for adv.
        public string GetMarketOwner()
        {
            CustomerContact Owner =
                MyCustomMarketService.GetOwner(
                _currentMarket.GetCurrentMarket().MarketId.Value, out bool foundOne);

            if (foundOne)
            {
                return Owner.FullName;
            }
            else
            {
                return "No market owner found";
            }

        }

        protected static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        protected static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }

    }
}
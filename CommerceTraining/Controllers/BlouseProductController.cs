using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.ServiceLocation;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;
using EPiServer.Commerce.Catalog;
using Mediachase.Commerce.Catalog;
using System.Globalization;
using EPiServer.DataAccess;
using CommerceTraining.Models.ViewModels;
using Mediachase.Commerce;
//using CommerceTraining.Models.Catalog;

namespace CommerceTraining.Controllers
{
    public class BlouseProductController : CatalogControllerBase<BlouseProduct>
    {
        //private ILinksRepository _linksRepository;
        private ReferenceConverter _referenceConverter;
        private IRelationRepository _relationRepository;
        private ICurrentMarket _currentMarket;

        public BlouseProductController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver
            , AssetUrlConventions assetUrlConvensions
            //, ILinksRepository linksRepository // deprecated
            , IRelationRepository relationRepository
            , ReferenceConverter referenceConverter
            ,ICurrentMarket currentMarket
        )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver,currentMarket)
        {
            //_linksRepository = linksRepository;
            _relationRepository = relationRepository;
            _referenceConverter = referenceConverter;
            _currentMarket = currentMarket;
        }

        public ActionResult Index(BlouseProduct currentContent, CatalogRoutingStartPage currentPage)
        {
            IEnumerable<ContentReference> variationRefs = currentContent.GetVariants();
            IEnumerable<EntryContentBase> variations =
                _contentLoader.GetItems(variationRefs, new LoaderOptions()).OfType<EntryContentBase>();

            
            var model = new BlouseProductViewModel(currentContent, currentPage)
            {
                productVariations = variations, // ECF 
                campaignLink = currentPage.PromoPage // CMS
            };

            return View(model);
        }

        public void CreateWithCode() // Maybe not the right place to put this code...
        {
            string nodeName = "myNode";
            string productName = "myProduct";
            string skuName = "mySku";

            // Create Node
            ContentReference linkToParentNode = _referenceConverter.GetContentLink("Women_1");
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var newNode = contentRepository.GetDefault<FashionNode>(linkToParentNode, new CultureInfo("en"));
            newNode.Code = nodeName;
            newNode.SeoUri = nodeName;
            newNode.Name = nodeName;
            newNode.DisplayName = nodeName;

            ContentReference newNodeRef = contentRepository.Save
                (newNode, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            // Create Product
            var newProduct = contentRepository.GetDefault<BlouseProduct>(newNodeRef, new CultureInfo("en"));

            //Set some properties.
            newProduct.Code = productName;
            newProduct.SeoUri = productName;
            newProduct.Name = productName;
            newProduct.DisplayName = productName;
            newProduct.SeoInformation.Title = "SEO Title";
            newProduct.SeoInformation.Keywords = "Some keywords";
            newProduct.SeoInformation.Description = "A nice one";
            newProduct.MainBody = new XhtmlString("This new product is great");

            // Persist the Product
            ContentReference newProductReference = contentRepository.Save
                (newProduct, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            // Create SKU
            var newSku = contentRepository.GetDefault<ShirtVariation>(newNodeRef, new CultureInfo("en"));

            newSku.Code = skuName;
            newSku.SeoUri = skuName;
            newSku.Name = skuName;
            newSku.DisplayName = skuName;
            //newSku.ParentLink = newProductReference; // Can do this in 10 - will change in 11

            ContentReference newSkuReference = contentRepository.Save
                (newSku, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            //what differs from CMS
           ProductVariation prodVariationLink0 = new ProductVariation()
           {
               Child = newSkuReference,
               Parent = newProductReference,
               SortOrder = 100
           };

            _relationRepository.UpdateRelation(prodVariationLink0);

        }

        
    }
}
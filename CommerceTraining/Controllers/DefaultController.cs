using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Catalog;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce;
using EPiServer.Web.Routing;
using EPiServer.Commerce.Catalog;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;

namespace CommerceTraining.Controllers
{
    public class DefaultController : CatalogControllerBase<DefaultVariation>
    {


        //private readonly IContentLoader _contentLoader; // hides the base
        private readonly IPriceService _priceService;
        private readonly IPriceDetailService _priceDetailService;
        private readonly ICurrentMarket _currentMarket;

        public CatalogKey MyCatalogKey { get; set; }

        public DefaultController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver // use this in node listing instead
            , IPriceService priceService
            , IPriceDetailService pricedetailService
            , ICurrentMarket currentMarket
            )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver, currentMarket)
        {
            //_contentLoader = contentLoader; // hides the base
            _priceService = priceService;
            _priceDetailService = pricedetailService;
            _currentMarket = currentMarket;

        }

        public ActionResult Index(DefaultVariation currentContent) // currentPage works
        {
            /* Implementation of action. You can create your own view model class that you pass to the view or
             * you can pass the page type for simpler templates */

            MyCatalogKey = new CatalogKey(currentContent.Code);

            return View(currentContent);
        }

        public IEnumerable<IPriceValue> GetThePrices()
        {
            return _priceService.GetCatalogEntryPrices(MyCatalogKey);
        }

        public IPriceValue GetTheRigtPrice() // depending on inventory level
        {

            return null;
        }


        Injected<IInventoryService> inventoryService;
        private CustomerPricing CheckInventory(DefaultVariation currentContent)
        {
            CustomerPricing localcheck = new CustomerPricing();

            // need some logic to pick the right price-group
            // Could have DefaultPrice, BatchPrice and WebPrice ... and groups accordingly to those prices

            // if PreOrder (the batch qty) > 
            InventoryRecord record = inventoryService.Service.Get(currentContent.Code, "London");
            var a = record.AdditionalQuantity; // (now Reserved... not used in bizlogic API says...but it is)
            //var x = record.a

            /* Suggestion:
             * Having a prop at the sku like BatchQty set to e.g. 100
             * Having a prop at the SKU for original inventory
             * when PurchaseAvailable reach that number we revert to the Default Price
             * ...have 10 in stock... gets a batch of 100 --> 110 in stock
             * When it reach 10 
            */
            return localcheck;
        }
    }
}
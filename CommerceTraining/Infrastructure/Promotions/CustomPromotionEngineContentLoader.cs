using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using Mediachase.Commerce;
using EPiServer.Commerce.Order.Internal;
using CommerceTraining.Infrastructure.Pricing;
using Mediachase.Commerce.Pricing;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce.Catalog;

namespace CommerceTraining.Infrastructure.Promotions
{

    public class CustomPromotionEngineContentLoader : PromotionEngineContentLoader
    {
        private IContentLoader _contentLoader;
        private MyPriceCalculator _myPriceCalculator;

        public CustomPromotionEngineContentLoader(
            MyPriceCalculator myPriceCalculator,
            IContentLoader contentLoader,
            CampaignInfoExtractor campaignInfoExtractor,
            IPriceService readOnlyPricingLoader,
            ReferenceConverter referenceConverter)
            : base(contentLoader, campaignInfoExtractor
                  , readOnlyPricingLoader,referenceConverter)
        { _myPriceCalculator = myPriceCalculator;
            _contentLoader = contentLoader;
        }
        /// <summary>
        /// This is done for calculating a discounted price on "Customer Pricing" prices
        /// By default the "DefaultPrice is used"
        /// </summary>
        public override IOrderGroup CreateInMemoryOrderGroup(
            ContentReference entryLink
            , IMarket market
            , Currency marketCurrency)
        {
            // this is where you can reference your own pricing calculator to retrieve... 
            // ..."the right" sale price
            var theEntry = _contentLoader.Get<EntryContentBase>(entryLink);
            var orderGroup = new InMemoryOrderGroup(market, marketCurrency);
            //IPriceValue price = BestPricingCalculatorEver.GetSalePrice(entryLink);
            IPriceValue price = _myPriceCalculator.GetSalePrice(theEntry, 1);

            if (price != null && price.UnitPrice.Amount != 0)
            {
                orderGroup.Forms.First().Shipments.First().LineItems.Add(new InMemoryLineItem
                {
                    Quantity = 1,
                    Code = price.CatalogKey.CatalogEntryCode,
                    PlacedPrice = price.UnitPrice.Amount
                });
            }

            return orderGroup;
        }

    }
}
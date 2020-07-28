using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CommerceTraining.Infrastructure.Pricing
{
    /* Register in Init-mod.*/

    public class CustomPriceOptimizer : IPriceOptimizer
    {
        // ...can do a demo/exercise for this (in ECF Adv.)
        // ...also have code for "Arindams prices" (Key & Qty) in AdminPageController "sv" market
            // price differs in Qty (odd business rule, but true)
        // demo with the red shirt (higher price) (UK-market) and the white shirt original behaviour
        // also have "OddPricing" node and SKU 
        
        IContentLoader _contentLoader;
        ReferenceConverter _referenceConverter;
        DefaultPriceOptimizer _defaultPriceOptimizer;

        public CustomPriceOptimizer
            (
                IContentLoader contentLoader,
                ReferenceConverter referenceConverter,
                DefaultPriceOptimizer defaultPriceOptimizer
            )
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _defaultPriceOptimizer = defaultPriceOptimizer;
        }
        public IEnumerable<IOptimizedPriceValue> OptimizePrices(IEnumerable<IPriceValue> prices)
        {
            // ...look at the code
            var code = prices.First().CatalogKey.CatalogEntryCode;
            // get it...
            var theEntry = _contentLoader.Get<EntryContentBase>(_referenceConverter.GetContentLink(code));
            
            // get the nodes c-refs
            var theCategories = theEntry.GetCategories();

            // get the nodes
            var nodes = _contentLoader.GetItems(theCategories, new LoaderOptions());

            // is there in a specific node (like Arindam Prices bizz-reqs)?
            int aHit = nodes.Where(n => n.Name == "OddPricing").Count();
            // if aHit > 0 do the below

            // ...a silly example for diferentiating the price setup, see the trainer guidle lines...
            if (code == "Long-Sleeve-Shirt-Red-Large_1" | aHit != 0)
            {
                return prices.GroupBy(p => new
                {
                    p.CatalogKey,
                    p.MinQuantity,
                    p.MarketId,
                    p.ValidFrom,
                    p.CustomerPricing,
                    p.UnitPrice.Currency
                })
                    .Select(g => g.OrderByDescending(c => c.UnitPrice.Amount)
                    .First()).Select(p => new OptimizedPriceValue(p, null));
            }
            else
            {
                return _defaultPriceOptimizer.OptimizePrices(prices);
            }
        }
    }
}


using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.Pricing
{
    public class DemoPriceOptimizer : IPriceOptimizer
    {
        private DefaultPriceOptimizer _defaultPriceOptimzer;
        public DemoPriceOptimizer(DefaultPriceOptimizer defaultPriceOptimizer)
        {
            _defaultPriceOptimzer = defaultPriceOptimizer;
        }
        public IEnumerable<IOptimizedPriceValue> OptimizePrices(IEnumerable<IPriceValue> prices)
        {
            var code = prices.First().CatalogKey.CatalogEntryCode;

            if (code == "Long Sleeve Shirt White Small_1")
            {
                return prices.GroupBy(p => new
                {
                    p.MarketId,
                    p.CustomerPricing,
                    p.MinQuantity
                })
                .Select(g => g.OrderByDescending(c => c.UnitPrice.Amount).First())
                .Select(p => new OptimizedPriceValue(p, null));
            }
            else return _defaultPriceOptimzer.OptimizePrices(prices);
        }
    }
}
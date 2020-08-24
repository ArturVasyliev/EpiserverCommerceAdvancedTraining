using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure
{
    public class DemoCurrentMarket : ICurrentMarket
    {
        public IMarket GetCurrentMarket()
        {
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            HttpCookie marketCookie = HttpContext.Current.Request.Cookies["marketCookie"];

            if(marketCookie == null)
            {
                return marketService.GetMarket(MarketId.Default);
            }

            string marketId = marketCookie["marketId"];

            return marketService.GetMarket(new MarketId(marketId));
        }

        public void SetCurrentMarket(MarketId marketId)
        {
            HttpCookie marketCookie = HttpContext.Current.Request.Cookies["marketCookie"];
            
            if(marketCookie == null)
            {
                marketCookie = new HttpCookie("marketCookie");
            }

            marketCookie["marketId"] = marketId.Value;
            marketCookie.Expires = DateTime.Now.AddMonths(1);
            HttpContext.Current.Response.Cookies.Add(marketCookie);

            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            var market = marketService.GetMarket(marketId);
            SiteContext.Current.Currency = market.DefaultCurrency;
        }
    }
}
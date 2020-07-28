using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace CommerceTraining.Infrastructure
{
    public class MyCustomCurrentMarket : ICurrentMarket
    {
        /* Note, changed from "profile" to "Cookie" */
        private const string _marketIdKey = "MarketId";
        private readonly IMarketService _marketService;
        private const string MarketCookie = "MarketId";
        private static readonly MarketId DefaultMarketId = new MarketId("DEFAULT").Value; // RoCe: change for the new 11
        private readonly MyCookieService _cookieService;
        
        // Init
        public MyCustomCurrentMarket(IMarketService marketService, MyCookieService cookieService)
        {
            _marketService = marketService;
            _cookieService = cookieService;
        }

        // Gets the Market selected for the current user, if the value is set and the indicated market is valid
        // ...otherwise, gets the default market
        // 1/2 done in starter file
        public IMarket GetCurrentMarket()
        {
            // ToDo: get the market
            var market = _cookieService.Get(MarketCookie);
            if (string.IsNullOrEmpty(market))
            {
                market = DefaultMarketId.Value;
            }
            return GetMarket(new MarketId(market));
        }


        // Sets the current market, if marketId represents a valid market;
        // ...otherwise, performs no action.
        // This will also set the current currency for the ECF context
        public void SetCurrentMarket(MarketId marketId)
        {
            // ToDo: Set the market
            var market = GetMarket(marketId);
            SiteContext.Current.Currency = market.DefaultCurrency;
            _cookieService.Set(MarketCookie, marketId.Value);
        }

        private IMarket GetMarket(MarketId marketId)
        {
            return _marketService.GetMarket(marketId) ?? _marketService.GetMarket(DefaultMarketId);
        }

        public IEnumerable<IMarket> GetAllMarkets() // if needed somewhere
        {
            // keep it simple
            IEnumerable<IMarket> allMarkets = _marketService.GetAllMarkets();

            var m = MarketId.Default;

            return allMarkets;
        }

        //  A Market having an "owner" that must be brought up along with the market
        // ...like B2B account manager, or a point of contact for the B2C visitor
        // ... moved to MyCustomMarketService 


    }
}
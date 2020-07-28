using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure
{
    public class CustomTaxCalculator : ITaxCalculator
    {
        private IContentRepository _contentRepository;
        private ReferenceConverter _referenceConverter;
        private readonly ITaxCalculator _defaultTaxCalculator;

        public CustomTaxCalculator(ITaxCalculator defaultTaxCalculator
            , IContentRepository contentRepository
            , ReferenceConverter referenceConverter)
        {
            _defaultTaxCalculator = defaultTaxCalculator;
            _contentRepository = contentRepository;
            _referenceConverter = referenceConverter;
        }

        [Obsolete("Don't use")]
        public Money GetReturnTaxTotal(IReturnOrderForm returnOrderForm, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetReturnTaxTotal(returnOrderForm, market, currency);
        }

        public Money GetSalesTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            // Silly example, but we may have use of an override
            _referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            decimal d = 0;
            List<ITaxValue> taxValues = new List<ITaxValue>();

            if (market.MarketId.Value == "sv" && shippingAddress.City == "Stockholm")
            {
                // Could have a generic "sv"-tax and... and onther higher for Stockholm (address.city)
                // ... then this example is not needed
                // Could set the City, Country etc. based on a IP-LookUp or something Market-wise
                // Could have the "Stockholm Market" where taxes and prices are higher
                // Like in The Netherlands "tourist accommodation tax" ... could differ between cities

                ContentReference contentLink = _referenceConverter.GetContentLink(lineItem.Code);
                IPricing pricingItem = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
                int i = (int)pricingItem.TaxCategoryId;
                string s = CatalogTaxManager.GetTaxCategoryNameById(i);

                taxValues.AddRange(OrderContext.Current.GetTaxes(new Guid(), s, "sv", shippingAddress));

                Money originalTax = _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, basePrice);

                foreach (var item2 in taxValues)
                {
                    // extra tax when shipped to Stockholm - doing the same with London and Tax-Jurisdiction
                    d += (decimal)(item2.Percentage + 0.10) * (lineItem.PlacedPrice * lineItem.Quantity);

                    // just a test
                    //Money originalTax2 = _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, basePrice);
                }
            }
            else
            {
                ContentReference contentLink = _referenceConverter.GetContentLink(lineItem.Code);
                IPricing pricingItem = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
                int i = (int)pricingItem.TaxCategoryId;
                string s = CatalogTaxManager.GetTaxCategoryNameById(i);

                // squiggles, but... 
                taxValues.AddRange(OrderContext.Current.GetTaxes(new Guid(), s, market.MarketId.Value, shippingAddress));

                // could use this
                var tm = TaxManager.GetTaxes(Guid.Empty, s, market.DefaultLanguage.TwoLetterISOLanguageName
                    , market.MarketId.Value, shippingAddress.RegionCode, null
                    , null, null, shippingAddress.City);

                foreach (var item2 in taxValues)
                {
                    // not Stockholm, so no extra city-tax
                    // ...doing with Tax-Jurisdictions for London and accesories (luxury item)
                    d += (decimal)(item2.Percentage) * (lineItem.PlacedPrice * lineItem.Quantity);
                }

                // just checking
                //Money originalTax3 = _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, new Money( lineItem.PlacedPrice,market.DefaultCurrency));
            }

            return new Money(d, market.DefaultCurrency) / 100;
        }

        [Obsolete("Don't use")]
        public Money GetShippingReturnTaxTotal(IShipment shipment, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetShippingReturnTaxTotal(shipment, market, currency);
        }

        public Money GetShippingTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            return _defaultTaxCalculator.GetShippingTax(lineItem, market, shippingAddress, basePrice);
        }

        [Obsolete("Don't use")]
        public Money GetShippingTaxTotal(IShipment shipment, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetShippingTaxTotal(shipment, market, currency);
        }

        public Money GetTaxTotal(IOrderGroup orderGroup, IMarket market, Currency currency)
        {
            return GetTaxTotal(orderGroup, market, currency);
        }

        public Money GetTaxTotal(IOrderForm orderForm, IMarket market, Currency currency)
        {
            return GetTaxTotal(orderForm, market, currency);
        }
    }
}
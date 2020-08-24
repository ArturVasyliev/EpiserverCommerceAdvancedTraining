using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using EPiServer.Core;
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
        private readonly IContentRepository _contentRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly ITaxCalculator _defaultTaxCalculator;

        public CustomTaxCalculator(ITaxCalculator defaultTaxCalculator
            , IContentRepository contentRepository
            , ReferenceConverter referenceConverter)
        {
            this._defaultTaxCalculator = defaultTaxCalculator;
            this._contentRepository = contentRepository;
            this._referenceConverter = referenceConverter;

        }

        [Obsolete("Don't use")]
        public Money GetReturnTaxTotal(IReturnOrderForm returnOrderForm, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetReturnTaxTotal(returnOrderForm, market, currency);
        }

        public Money GetSalesTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            //return _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, basePrice);
            //return new Money(1000 , "USD");

            // may have use of this
            // could have Cart/PO-property like ... "CheckTaxStandAlone" for forking code (used with the bogus-cart)
            //this._orderGroup = orderGroup;
            decimal d = 0;
            List<ITaxValue> taxValues = new List<ITaxValue>();

            if (market.MarketId.Value == "sv" && shippingAddress.City == "Stockholm")
            {

                #region Old stuff

                // need the shipment with the new stuff... else coding against the old stuff manually
                //IShipment ship = orderGroup.GetFirstShipment();


                // Could have a generic "sv"-tax and... and onther higher for Stockholm (address.city)
                // .. but then the below is not needed
                // could set the City, Country etc. based on a IP-LookUp or something Market-wise
                // Could have the "Stockholm Market" where taxes and prices are higher

                // ...just for testing
                //if (ship.ShippingAddress == null)
                //{
                //    this.address = _orderGroupFactory.Service.CreateOrderAddress(orderGroup);
                //    address.CountryCode = "sv";
                //    address.CountryName = "sv";

                //    // Like The Netherlands tourist accommodation tax ... differs between cities
                //    // when you set the city in CM-->Admin-->Taxes ... it gets excluded
                //    // and no tax is applied... have to find a "WorkAround"
                //    // the rest works...
                //    address.City = "Stockholm";
                //    address.Id = "DummyAddress";
                //    ship.ShippingAddress = address;
                //}

                //if (ship.ShippingAddress.City == "Stockholm")
                //{
                //    // 
                //    this.peopleFromStockholm = true;
                //}

                // Extra tax ...
                //if (ship.ShippingAddress.City == "Stockholm")
                //{
                //foreach (var item in orderGroup.GetAllLineItems())
                //{

                #endregion

                ContentReference contentLink = _referenceConverter.GetContentLink(lineItem.Code);

                IPricing pricingItem = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
                int i = (int)pricingItem.TaxCategoryId;
                string s = CatalogTaxManager.GetTaxCategoryNameById(i);

                //var t = TaxManager.GetTaxes(new Guid(), s, market.MarketId.Value, market.MarketId.Value, null, null, null, null, null);
                taxValues.AddRange(OrderContext.Current.GetTaxes(new Guid(), s, "sv", shippingAddress));
                // An address have to be there if using this ... so we can match the different properties
                //taxValues.AddRange(GetTaxValues(CatalogTaxManager.GetTaxCategoryNameById(i), "sv", shippingAddress));

                foreach (var item2 in taxValues)
                {
                    // extra tax when shipped to Stockholm 10% more
                    d += (decimal)(item2.Percentage + 0.10) * (lineItem.PlacedPrice * lineItem.Quantity);
                }
            }

            //}
            //var liAmount = orderGroup.Forms.Sum(x => x.GetAllLineItems().Sum(l => l.PlacedPrice * l.Quantity));
            //}
            else
            {
                //foreach (var item in orderGroup.GetAllLineItems())
                //{
                //ContentReference contentLink = _referenceConverter.GetContentLink(item.Code);

                
                foreach (var item2 in taxValues)
                {
                    // not Stockholm, so no extra tax
                    d += (decimal)(item2.Percentage) * (lineItem.PlacedPrice * lineItem.Quantity);
                }
                //}
            }

                return new Money(d, market.DefaultCurrency) / 100;
            }
            //else
            //{
            //    return _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, );// (orderGroup, market, currency);
            //}

        
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

        [Obsolete("Don't use")]
        public Money GetTaxTotal(IOrderGroup orderGroup, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetTaxTotal(orderGroup, market, currency);
        }

        [Obsolete("Don't use")]
        public Money GetTaxTotal(IOrderForm orderForm, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetTaxTotal(orderForm, market, currency);
        }
    }
}
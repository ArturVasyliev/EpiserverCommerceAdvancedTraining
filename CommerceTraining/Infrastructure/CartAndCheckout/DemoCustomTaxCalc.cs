using EPiServer;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    public class DemoCustomTaxCalc : ITaxCalculator
    {
        private ITaxCalculator _defaultTaxCalculator;

        public DemoCustomTaxCalc(ITaxCalculator defaultTaxCalculator)
        {
            _defaultTaxCalculator = defaultTaxCalculator;
        }

        [Obsolete("Don't use")]
        public Money GetReturnTaxTotal(IReturnOrderForm returnOrderForm, IMarket market, Currency currency)
        {
            return _defaultTaxCalculator.GetReturnTaxTotal(returnOrderForm, market, currency);
        }

        public Money GetSalesTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            if(market.MarketId.Value == "sv" && shippingAddress.City == "Stockholm")
            {
                decimal decPrice = 0;
                string taxCategory = CatalogTaxManager.GetTaxCategoryNameById((int)lineItem.TaxCategoryId);
                IEnumerable<ITaxValue> taxes = OrderContext.Current.GetTaxes(Guid.Empty, taxCategory, "sv", shippingAddress);

                foreach (var tax in taxes)
                {
                    decPrice += (decimal)(tax.Percentage + 0.10) * (lineItem.PlacedPrice * lineItem.Quantity);
                }
                return new Money(decPrice, basePrice.Currency) / 100;
            }
            else
            {
                return _defaultTaxCalculator.GetSalesTax(lineItem, market, shippingAddress, basePrice);
            }
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
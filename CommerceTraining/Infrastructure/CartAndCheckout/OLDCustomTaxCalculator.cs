using EPiServer.Commerce.Order.Calculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using EPiServer.ServiceLocation;
using EPiServer.Core;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce.Catalog.Managers;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    public class OLDCustomTaxCalculator : DefaultTaxCalculator // changes in ECF 12
    {   // should not grab the DefaultTaxCalculator... use "Intercept"
        // not done yet, a lot happened in 12 
        private ReferenceConverter _referenceConverter;
        private IContentRepository _contentRepository;
        private IOrderGroup _orderGroup;
        private bool peopleFromStockholm = false; // those guys will pay more tax
        private IOrderAddress address;

        public OLDCustomTaxCalculator(
            IContentRepository contentRepository
            , ReferenceConverter referenceConverter
            //, IShippingCalculator shippingCalculator // changes in ECF 12
            //, ILineItemCalculator lineItemCalculator // changes in ECF 12
            //, IReturnLineItemCalculator returnLineItemCalculator // changes in ECF 12
            )
            : base(contentRepository
                  , referenceConverter
                  //, shippingCalculator
                  //, lineItemCalculator
                  //, returnLineItemCalculator
                  )
        {
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
        }

        Injected<ICurrentMarket> _currMarket;
        // Here is one place to plug in your custom stuff
        protected override IEnumerable<ITaxValue> GetTaxValues(string taxCategory
            , string languageCode, IOrderAddress orderAddress)
        {

            var theMarket = _currMarket.Service.GetCurrentMarket();

            if (theMarket.MarketId.Value.ToString() != "sv")
            {
                return base.GetTaxValues(taxCategory, languageCode, orderAddress);
            }
            else
            {
                // get it from some other place
                //return new[] { new CustomTaxValue(50,"sv","Too High",TaxType.SalesTax,) };
                return base.GetTaxValues(taxCategory, languageCode, orderAddress);
            }

        }

        // ECF 12
        protected override Money CalculateSalesTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            return base.CalculateSalesTax(lineItem, market, shippingAddress, basePrice);
        }

        // ECF 12
        protected override Money CalculateShippingTax(ILineItem lineItem, IMarket market, IOrderAddress shippingAddress, Money basePrice)
        {
            return base.CalculateShippingTax(lineItem, market, shippingAddress, basePrice);
        }

        // use this one to get shipping taxes in a custom fashion
        protected override Money CalculateShippingTaxTotal(IShipment shipment, IMarket market, Currency currency)
        {
            bool doCustom = true;

            if (!doCustom)
            {
                return new Money(0, currency);
            }
            else
            {
                return base.CalculateShippingTaxTotal(shipment, market, currency);
            }
        }

        // ...could maybe have use of this... with custom bizz-rules
        Injected<IOrderGroupFactory> _orderGroupFactory;

        // Extra tax for Stockholm
        protected override Money CalculateTaxTotal(
            IOrderGroup orderGroup
            , IMarket market
            , Currency currency)
        {
            // may have use of this
            // could have Cart/PO-property like ... "CheckTaxStandAlone" for forking code (used with the bogus-cart)
            this._orderGroup = orderGroup;
            decimal d = 0;

            if (market.MarketId.Value == "sv")
            {
                // need the shipment with the new stuff... else coding against the old stuff manually
                IShipment ship = orderGroup.GetFirstShipment();
                List<ITaxValue> taxValues = new List<ITaxValue>();

                // Could have a generic "sv"-tax and... and onther higher for Stockholm (address.city)
                // .. but then the below is not needed
                // could set the City, Country etc. based on a IP-LookUp or something Market-wise
                // Could have the "Stockholm Market" where taxes and prices are higher

                // ...just for testing
                if (ship.ShippingAddress == null)
                {
                    this.address = _orderGroupFactory.Service.CreateOrderAddress(orderGroup);
                    address.CountryCode = "sv";
                    address.CountryName = "sv";

                    // Like The Netherlands tourist accommodation tax ... differs between cities
                    // when you set the city in CM-->Admin-->Taxes ... it gets excluded
                    // and no tax is applied... have to find a "WorkAround"
                    // the rest works...
                    address.City = "Stockholm";
                    address.Id = "DummyAddress";
                    ship.ShippingAddress = address;
                }

                if (ship.ShippingAddress.City == "Stockholm")
                {
                    // 
                    this.peopleFromStockholm = true;
                }

                // Extra tax ...
                if (ship.ShippingAddress.City == "Stockholm")
                {
                    foreach (var item in orderGroup.GetAllLineItems())
                    {
                        ContentReference contentLink = _referenceConverter.GetContentLink(item.Code);

                        IPricing pricing = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
                        int i = (int)pricing.TaxCategoryId;

                        // An address have to be there if using this ... so we can match the different properties
                        taxValues.AddRange(GetTaxValues(CatalogTaxManager.GetTaxCategoryNameById(i), "sv", address));

                        foreach (var item2 in taxValues)
                        {
                            // extra tax when shipped to Stockholm 10% more
                            d += (decimal)(item2.Percentage + 0.10) * (item.PlacedPrice * item.Quantity);
                        }
                    }
                    //var liAmount = orderGroup.Forms.Sum(x => x.GetAllLineItems().Sum(l => l.PlacedPrice * l.Quantity));
                }
                else
                {
                    foreach (var item in orderGroup.GetAllLineItems())
                    {
                        ContentReference contentLink = _referenceConverter.GetContentLink(item.Code);

                        IPricing pricing = _contentRepository.Get<EntryContentBase>(contentLink) as IPricing;
                        int i = (int)pricing.TaxCategoryId;

                        // An address have to be there
                        taxValues.AddRange(GetTaxValues(CatalogTaxManager.GetTaxCategoryNameById(i), "sv", address));

                        foreach (var item2 in taxValues)
                        {
                            // not Stockholm, so no extra tax
                            d += (decimal)(item2.Percentage) * (item.PlacedPrice * item.Quantity);
                        }
                    }
                }

                return new Money(d, market.DefaultCurrency) / 100;
            }
            else
            {
                return base.CalculateTaxTotal(orderGroup, market, currency);
            }
        }

        protected override string GetTaxCategoryNameById(int taxCategoryId)
        {
            return base.GetTaxCategoryNameById(taxCategoryId);
        }

    }
}
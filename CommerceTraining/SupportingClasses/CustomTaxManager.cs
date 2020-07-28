using EPiServer.Commerce.Order;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public class CustomTaxManager
    {
        Injected<ITaxCalculator> _taxCalc;
        Injected<ICurrentMarket> _currMark;
        Injected<IShippingCalculator> _ShipCalc;

        public void CheckTaxes(ICart thecart)
        {
            //...just checking

            // After ECF 12
            _ShipCalc.Service.GetShippingTax(thecart.GetFirstShipment()
                , _currMark.Service.GetCurrentMarket(), thecart.Currency);

            _ShipCalc.Service.GetSalesTax(thecart.GetFirstShipment()
                , _currMark.Service.GetCurrentMarket(), thecart.Currency);

            // Before 12
            Money theTax = _taxCalc.Service.GetShippingTaxTotal(
                thecart.GetFirstShipment(), _currMark.Service.GetCurrentMarket(), thecart.Currency);

            _taxCalc.Service.GetTaxTotal(
                thecart.GetFirstForm(), thecart.Market, thecart.Currency);
        }

        // ...still what's used in the "newer calculators"
        public TaxValue[] GetTaxes(int taxCategoryId)
        {
            TaxValue[] taxes = null;

            // get the cat-string
            string taxCategory = CatalogTaxManager.GetTaxCategoryNameById(taxCategoryId);

            // if we don't have a full address, TaxManager - no squiggles
            taxes = OrderContext.Current.GetTaxes
                (Guid.Empty, taxCategory, GetLanguageName(), GetCountryCode()
                , String.Empty, String.Empty, String.Empty, String.Empty
                , String.Empty);
            
            return taxes;
        }

        private string GetLanguageName()
        {
            // not pretty, will change
            return GetCountryCode(); // could perhaps change this

        }

        // plumbing for returning a country code to get TaxValues
        public string GetCountryCode()
        {
            // instead of using an OrderAddress
            string countryCode = String.Empty;

            if (GetMarketName() != "Default Market")
            {
                // doing it by market is better
                countryCode = _currMark.Service.GetCurrentMarket().MarketId.Value;

            }
            else // DEFAULT market... in other words, no active choice of Market
            {
                countryCode = EPiServer.Globalization.ContentLanguage.PreferredCulture.Name;
            }

            return countryCode;
        }

        public string GetMarketName()
        {
            return _currMark.Service.GetCurrentMarket().MarketName;
        }
    }
}
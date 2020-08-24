using EPiServer.Commerce.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mediachase.Commerce.Orders;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    public class CustomTaxValue : ITaxValue
    {
        public CustomTaxValue()
        {


        }
        public string OtherProperty { get; set; }

        public string CustomStuff { get; set; }

        public double Percentage => SetPercentage();

        private double SetPercentage()
        {
            double d = 0;
            bool b = false;

            if (b)
            {
                return d;
            }
            else
            {
                return d;
            }
        }

        public string Name => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public TaxType TaxType => SpecialTaxTypes();

        private TaxType SpecialTaxTypes() 
        {
            return TaxType.SalesTax;
        }

    }
     
    public enum MoreTaxTypes
    {


    }
}
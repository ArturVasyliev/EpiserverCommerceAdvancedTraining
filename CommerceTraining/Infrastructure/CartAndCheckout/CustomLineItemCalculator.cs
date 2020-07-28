using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    // not done with this yet
    public class CustomLineItemCalculator : DefaultLineItemCalculator
    {
        public CustomLineItemCalculator(ITaxCalculator taxCalculator)
            :base(taxCalculator)
        { }
        protected override void ValidateExtendedPrice(Money money)
        {
            // if no "default price" is set, the GetDefaultPrice() --> 0
            // but, now we can add a line item with qty 0, but if that is not permitted...
            // could be the scope of a WishList instead

            if (money.Amount <= decimal.Zero) 
            {
                throw new ValidationException("Price cannot be zero or below");
            }
        }

        protected override Money CalculateExtendedPrice(ILineItem lineItem, Currency currency)
        {
            return base.CalculateExtendedPrice(lineItem, currency);
        }

    }
}
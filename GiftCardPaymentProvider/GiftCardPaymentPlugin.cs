using EPiServer.Commerce.Order;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GiftCardPaymentProvider
{
    public class GiftCardPaymentPlugin : IPaymentPlugin
    {
        public IDictionary<string, string> Settings { get; set; }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            payment.TransactionType = TransactionType.Sale.ToString();
            var result = GiftCardService.DebitGiftCard(Settings["GiftCardMetaClassName"], (PrimaryKeyId)orderGroup.CustomerId, payment.ValidationCode, payment.Amount);
            if (result)
            {
                return PaymentProcessingResult.CreateSuccessfulResult($"Gift Card Applied for {payment.Amount}!");
            }
            else
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult("Gift Card Declined!");
            }
        }
    }
}
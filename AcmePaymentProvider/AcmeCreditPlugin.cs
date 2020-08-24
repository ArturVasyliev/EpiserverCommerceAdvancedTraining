using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AcmePaymentProvider
{
    public class AcmeCreditPlugin : IPaymentPlugin
    {
        public IDictionary<string, string> Settings { get; set; }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            decimal CreditLimit = 500;
            string secretKey = Settings["SecretKeyExample"];
            payment.TransactionType = TransactionType.Sale.ToString();
            if (payment.Amount <= CreditLimit)
            {
                return PaymentProcessingResult.CreateSuccessfulResult($"Acme credit approved payment for {payment.Amount}! Secret Code: {secretKey}");
            }
            else
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult($"Sorry, you are over your limit! Secret Code: {secretKey}");
            }
        }
    }
}
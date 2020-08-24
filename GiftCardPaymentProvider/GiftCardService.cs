using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GiftCardPaymentProvider
{
    public static class GiftCardService
    {
        public static EntityObject[] GetClientGiftCards(string giftCardMetaClass, PrimaryKeyId contactId)
        {
            return BusinessManager.List(giftCardMetaClass,
                new FilterElement[]
                {
                    FilterElement.EqualElement("ContactId", contactId),
                    FilterElement.EqualElement("IsActive", true)
                });
        }

        public static bool DebitGiftCard(string giftCardMetaClass, PrimaryKeyId contactId, string redemtionCode, decimal debitAmount)
        {
            var cards = BusinessManager.List(giftCardMetaClass,
                new FilterElement[]
                {
                    FilterElement.EqualElement("ContactId", contactId),
                    FilterElement.EqualElement("IsActive", true),
                    FilterElement.EqualElement("RedemtionCode", redemtionCode)
                });
            if (cards == null || cards.Count() == 0) return false;
            var card = cards[0];
            if ((decimal)card["Balance"] < debitAmount) return false;
            decimal newBalance = (decimal)card["Balance"] - debitAmount;
            card["Balance"] = newBalance;
            if (newBalance == 0) card["IsActive"] = false;
            BusinessManager.Update(card);

            return true;
        }
    }
}
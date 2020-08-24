using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure
{
    public class RemoveMeWhenCleaningUp
    {
        #region From VariationController

        //private string GetAssociationMetaData(EntryContentBase currentContent)
        //{
        //    // not good, find a better way than do this twice
        //    IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();
        //    StringBuilder strB = new StringBuilder();

        //    if (assoc.Count() >= 1)
        //    {
        //        // Fix code and formatting, but it works
        //        EPiServer.Commerce.Catalog.Linking.Association a = assoc.FirstOrDefault(); // get the only one .. so far for test
        //        strB.Append("Group-Name: " + a.Group.Name);
        //        strB.Append(" - ");
        //        strB.Append("Group-Description: " + a.Group.Description);
        //        strB.Append(" - ");
        //        strB.Append("Group-Sort: " + a.Group.SortOrder);
        //        strB.Append(" - ");
        //        strB.Append("Type-Id: " + a.Type.Id); // where the filter could be applied
        //        strB.Append(" - ");
        //        strB.Append("Type-Descr: " + a.Type.Description);
        //        // there is more to get out
        //        ContentReference theRef = a.Target;
        //        strB.Append("...in VariationController");
        //    }
        //    else
        //    {
        //        strB.Append("Nothing");
        //    }

        //    return strB.ToString();
        //}

        // ToDo: clean up here ... have an Aggergation for this part ... could use as demo

        //private IEnumerable<ContentReference> GetAssociatedEntries(EntryContentBase currentContent)
        //{
        //    // using linksRep.
        //    ILinksRepository _linksRep = ServiceLocator.Current.GetInstance<ILinksRepository>();

        //    IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> linksRepAssoc =
        //        _linksRep.GetAssociations(currentContent.ContentLink).Where(l => l.Group.Name == "CrossSell");
        //    // would like to be able to filter when calling, instead of .Where()

        //    // would like to get the metadata out ... like type and group... and probaly treat them differently
        //    IEnumerable<EPiServer.Commerce.Catalog.Linking.Association> assoc = currentContent.GetAssociations();

        //    List<ContentReference> refs = new List<ContentReference>();

        //    foreach (EPiServer.Commerce.Catalog.Linking.Association item in assoc)
        //    {
        //        refs.Add(item.Target);
        //    }

        //    return refs;
        //}


        //private decimal CheckBetaPromotions(ShirtVariation currentContent, out string rewardDescription) // feb/mar
        //{
        //    CartHelper ch = new CartHelper("fakeCart");
        //    ch.AddEntry(currentContent.LoadEntry(), 6, true, new CartHelper[] { });

        //    LineItem li = ch.AddEntry(currentContent.LoadEntry());

        //    var dtoShip = ShippingManager.GetShippingMethodsByMarket
        //        (_currentMarket.GetCurrentMarket().MarketId.Value, false).ShippingMethod.FirstOrDefault();
        //    Shipment s = new Shipment();
        //    s.ShippingMethodId = dtoShip.ShippingMethodId;
        //    s.ShippingMethodName = dtoShip.Name;
        //    int ShipId = ch.Cart.OrderForms.First().Shipments.Add(s);

        //    IOrderGroup newCart = (IOrderGroup)ch.Cart;
        //    newCart.Forms.First().Shipments.First().LineItems.Add(ch.LineItems.First());
        //    newCart.Forms.First().Shipments.First().LineItems.Add((ILineItem)li);

        //    IEnumerable<RewardDescription> rewards =
        //        ServiceLocator.Current.GetInstance<IPromotionEngine>().Run(newCart); // fix later

        //    // just playing around
        //    decimal savedMoney = 0;
        //    rewardDescription = String.Empty;
        //    /*foreach (RewardDescription item in rewards)
        //    {
        //        if (item.RewardType == Mediachase.Commerce.Marketing.RewardType.Percentage)
        //        {
        //            if (item.Status == Mediachase.Commerce.Marketing.FulfillmentStatus.Fulfilled)
        //            {
        //                rewardDescription = item.Description;
        //                savedMoney = item.Percentage * currentContent.GetDefaultPrice().UnitPrice.Amount/100;
        //            }
        //        }
        //    }*/

        //    return savedMoney;
        //}




        #endregion

        #region From ccService

        #region Older stuff
        // Old
        //public void IsSecondShipmentReqired(CartHelper ch)
        //{
        //    foreach (LineItem item in ch.LineItems)
        //    {
        //        if ((bool)item["RequireSpecialShipping"] == true)
        //        {
        //            ch.Cart["SpecialShip"] = true;
        //            break;
        //        }
        //        else
        //        {
        //            ch.Cart["SpecialShip"] = false;
        //        }
        //    }
        //}

        //New - sets SpecialShip on the Cart

        // need to set the CheckBox ... else crash
        // Changed the signature of this one


        //// Old ... delete?
        //public void AddSecondPaymentToOrder(Cart cart, Payment newPayment)
        //{
        //    // ToDo: Add a second payment
        //    OtherPayment giftCardPayment = null;

        //    // Split payment here (...no more the WF is taking care of things for us)
        //    // Subtract the amount at the giftcard for "original" payment... "we just empty the card"... as a demo/lab
        //    newPayment.Amount = cart.Total - decimal.Parse(this.theGiftCard["Balance"].ToString());

        //    // Add "GiftCard" payment to the Cart
        //    giftCardPayment = new OtherPayment();

        //    // this and balance management (at the end) can be improved with more creative logic
        //    giftCardPayment.Amount = decimal.Parse(this.theGiftCard["Balance"].ToString());

        //    // Note: the below payment method doesn´t exist, we do like this for lab/demo
        //    giftCardPayment.PaymentMethodName = "GiftCard";

        //    // Comment: we need a "true" payment type (and method), gets an eror with an empty/not stored Guid
        //    // Could transform the GiftCard to a "true" payment ...in the same way as "PayMe" or the lab in 2840
        //    // So, we´re "Borrowing" a payment guid (saves time, but would like a "real type")
        //    PaymentMethodDto payDto = PaymentManager.GetPaymentMethodBySystemName
        //        ("Generic", ContentLanguage.PreferredCulture.Name);

        //    giftCardPayment.PaymentMethodId = payDto.PaymentMethod[0].PaymentMethodId;
        //    // ...allmost okay for this demo/lab

        //    // could wrap a lot of what´s done here in a transaction scope...
        //    cart.OrderForms[0].Payments.Add(giftCardPayment);
        //    // Could use the OrderContext instead of CartHelper... all over!

        //    // ToDo: Maybe add identification of the card in the Order 

        //    // Can have more finesse here, the lab just buys for more than the card balance...
        //    // ...to force a split payment
        //    // Reset and deactivate the giftcard if it´s now empty (in the lab it is)
        //    this.theGiftCard["IsActive"] = false;
        //    this.theGiftCard["Balance"] = 0M;

        //    // persist the card new details
        //    BusinessManager.Update(this.theGiftCard);
        //}

        // New

        //// Old ... delete?
        //public void AddSecondPaymentToOrder(Cart cart, Payment newPayment)
        //{
        //    // ToDo: Add a second payment
        //    OtherPayment giftCardPayment = null;

        //    // Split payment here (...no more the WF is taking care of things for us)
        //    // Subtract the amount at the giftcard for "original" payment... "we just empty the card"... as a demo/lab
        //    newPayment.Amount = cart.Total - decimal.Parse(this.theGiftCard["Balance"].ToString());

        //    // Add "GiftCard" payment to the Cart
        //    giftCardPayment = new OtherPayment();

        //    // this and balance management (at the end) can be improved with more creative logic
        //    giftCardPayment.Amount = decimal.Parse(this.theGiftCard["Balance"].ToString());

        //    // Note: the below payment method doesn´t exist, we do like this for lab/demo
        //    giftCardPayment.PaymentMethodName = "GiftCard";

        //    // Comment: we need a "true" payment type (and method), gets an eror with an empty/not stored Guid
        //    // Could transform the GiftCard to a "true" payment ...in the same way as "PayMe" or the lab in 2840
        //    // So, we´re "Borrowing" a payment guid (saves time, but would like a "real type")
        //    PaymentMethodDto payDto = PaymentManager.GetPaymentMethodBySystemName
        //        ("Generic", ContentLanguage.PreferredCulture.Name);

        //    giftCardPayment.PaymentMethodId = payDto.PaymentMethod[0].PaymentMethodId;
        //    // ...allmost okay for this demo/lab

        //    // could wrap a lot of what´s done here in a transaction scope...
        //    cart.OrderForms[0].Payments.Add(giftCardPayment);
        //    // Could use the OrderContext instead of CartHelper... all over!

        //    // ToDo: Maybe add identification of the card in the Order 

        //    // Can have more finesse here, the lab just buys for more than the card balance...
        //    // ...to force a split payment
        //    // Reset and deactivate the giftcard if it´s now empty (in the lab it is)
        //    this.theGiftCard["IsActive"] = false;
        //    this.theGiftCard["Balance"] = 0M;

        //    // persist the card new details
        //    BusinessManager.Update(this.theGiftCard);
        //}

        // New

        #endregion
        
        #endregion
    }
}
using CommerceTraining.Models.Catalog;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using System.Data;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    public class CartAndCheckoutService
    {
        protected EntityObject TheGiftCard { get; set; }
        public string FrontEndMessage { get; set; }
        
        Injected<IOrderGroupFactory> _orderGroupFactory;
        Injected<IOrderRepository> _orderRepository;
        Injected<ICurrentMarket> _currentMarket;
        public CartAndCheckoutService
            (
            //ICurrentMarket currentMarket
            //IContentLoader contentLoader
            )
        {
            // May need this
            //_contentLoader = contentLoader;
            //_currentMarket = currentMarket;
        }


        #region Checking on "CleanUpCart"

        #endregion

        #region SplitShip

        public void CleanOutPayments(ICart cart)
        {
            Dictionary<int, IPayment> payments = new Dictionary<int, IPayment>();

            foreach (var cartForm in cart.Forms)
            {
                foreach (var payment in cartForm.Payments)
                {
                    payments.Add(cartForm.OrderFormId, payment);
                }
            }

            if (payments.Count >= 1)
            {
                var ofp = cart.Forms.GroupBy(f => f.OrderFormId, f => f.Payments);

                foreach (var item in ofp)
                {
                    IOrderForm f = cart.Forms.Where(x => x.OrderFormId == item.Key).First();
                    foreach (var item2 in f.Payments)
                    {
                        f.Payments.Remove(item2);
                    }
                }
            }
        }

        
        public bool CheckOnLineItem(DefaultVariation variation, ILineItem li)
        {
            // need to set it to something for the serialized carts, so the property becomes created
            if (variation.RequireSpecialShipping)
            {
                li.Properties["LineItemSpecialShipping"] = true;
            }
            else
            {
                li.Properties["LineItemSpecialShipping"] = false;
            }

            return variation.RequireSpecialShipping;
        }

        // Generic one 
        public bool CheckOnLineItems(ICart cart)
        {
            // need to set it to something
            var lineItems = cart.GetAllLineItems();
            //bool theBool = false;
            foreach (var item in lineItems)
            {
                if ((bool)item.Properties["LineItemSpecialShipping"] == true)
                {
                    cart.Properties["SpecialShip"] = true;
                }
                else
                {
                    cart.Properties["SpecialShip"] = false;
                }
            }

            return (bool)cart.Properties["SpecialShip"];
        }

        // New Jan 2018 - do in another way and return the guid
        public Guid CheckApplicableShippingsNew(string code)
        {
            #region New
            
            Guid theShippingMethodGuid = new Guid();

            // just playing around
            // Not any good API for getting ShippingOptionParameters direct

            List<FilterElement> filters = new List<FilterElement>();
            filters.Add(new FilterElement("Value", FilterElementType.Equal, code)); // the Code

            Guid theGuid = new Guid();
            using (IDataReader reader = Mediachase.BusinessFoundation.Data.DataHelper
                .List("ShippingMethodParameter", filters.ToArray()))
            {
                while (reader.Read())
                {
                    if (reader["Value"].ToString() == code)
                    {
                        theGuid = new Guid(reader["ShippingMethodId"].ToString());
                    }

                }
                
            }

            return theGuid;

            #endregion

            #region Previous

            /* load the Shipping-Method-Parameters and inspect what method will be used... 
             * for now, just output text to the page, could also use it for loading "the right" method-guid
             *   for the second shipment added */

            /*
            string str = String.Empty;

            // get the rows, we don't care about the market-part now
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket
                (MarketId.Default.Value, false);

            // Have this one also, but it's harder (ref. Shannon's blog - Order Search)
            // ShippingManager.GetShipments(...)
            // would like to index the "right method" guid directly on the Entry

            foreach (ShippingMethodDto.ShippingMethodRow item in dto.ShippingMethod)
            {
                // This is the interesting part, could be complemented with dbo.ShippingOptionParameter
                // get the param-rows, could show the dbo.ShippingMethodParameter table in VS
                ShippingMethodDto.ShippingMethodParameterRow[] paramRows =
                    item.GetShippingMethodParameterRows();

                if (paramRows.Count() != 0)
                {
                    // right now we're just outputting text to the cart
                    //   ... should more likely get the guid instead
                    // could be here we can match lineItem with the shipping method or gateway
                    foreach (var item2 in paramRows)
                    {
                        str += item.Name + " : " + item2.Parameter + " or ";
                        var v = item2.Value; // ...not in use... yet...
                    }
                }
            }
            return str;
            */
            #endregion

            //return theShippingMethodGuid;
        }


        // ...for now we do it in the TrousersController (RoCe: re-write when time permits)
        // not in use yet

        /* This method is not done yet, and not in use
         * ...will consolidate all this about a forced split shipment...it's spread out now. */
        public ICart AddAnotherShipment(ICart cart, ILineItem lineItem, bool reuseAddress)
        {
            // need to check and add what shipment to use
            string forcedShipmentName = String.Empty; // ...if needed
            Guid shippingOptionGuid = new Guid(); // for the Shipping Gateway

            // RoCe: move from the Trouser-Controller
            // Create Address & Sipment
            if (!reuseAddress)
            {
                IOrderAddress secondAddress = _orderGroupFactory.Service.CreateOrderAddress(cart);
            }
            else
            {
                IOrderAddress secondAddress = cart.GetFirstForm().Shipments.First().ShippingAddress;
            }

            // get the whole dto so we can try to find the ShippingOptionParameters set
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(
                _currentMarket.Service.GetCurrentMarket().MarketId.Value, false);

            var shippingOptionParam = dto.ShippingOptionParameter; // separate table - points to the Gateway

            // look for params (separate table) ... gets the gateway guid
            foreach (var item in shippingOptionParam)
            {
                if (item.Parameter == "Suspenders") // ...a string, as illustration... to find out what Gateway/Method to use
                {
                    shippingOptionGuid = item.ShippingOptionId; // ShippingGateway ... need the id of RoyalMail set when created
                    forcedShipmentName = item.Value;
                }
            }

            // now we know what ShippingMethod to use (shippingOptionGuid)
            ShippingMethodDto.ShippingMethodRow foundShipping = null;
            foreach (var item in dto.ShippingMethod) // ECF should maybe have a method for this (if a 1:1 match is common)
            {
                if (item.ShippingOptionId == shippingOptionGuid)
                {
                    foundShipping = ShippingManager.GetShippingMethod(item.ShippingMethodId).ShippingMethod.FirstOrDefault();
                }
            }

            // could be furter granular eg. what type of method for the Gateway ... like "Standard" or "Express"
            ShippingMethodDto.ShippingMethodParameterRow[] paramRows = foundShipping.GetShippingMethodParameterRows();

            // here it just gets to the front-end as string
            if (paramRows.Count() != 0)
            {
                foreach (var item2 in paramRows) // could be here we can match lineItem with ...
                {
                    var p = item2.Parameter;
                    var v = item2.Value;
                }
            }

            return cart; // dummy for now
        }

        #endregion

        #region SplitPay 
        public void AddSecondPaymentToOrder(ICart cart)
        {
            // ToDo: get the first payment added
            IPayment oldPayment = cart.GetFirstForm().Payments.First();

            // ToDo: Add a second payment
            OtherPayment giftCardPayment = null;
            this.TheGiftCard = GetTheGiftCard();

            oldPayment.Amount = oldPayment.Amount - decimal.Parse(this.TheGiftCard["Balance"].ToString());

            // Add "GiftCard" payment to the Cart
            giftCardPayment = new OtherPayment
            {
                // this and balance management (at the end) can be improved with more creative logic
                Amount = decimal.Parse(this.TheGiftCard["Balance"].ToString()),

                // Note: the below payment method doesn´t exist, we do like this for lab/demo
                PaymentMethodName = "GiftCard"
            };

            // Comment: we should have a "true" payment type (and method), gets an eror with an empty/not stored Guid
            // Could transform the GiftCard to a "true" payment ...in the same way as "PayMe" or the demo/exercise in 2840
            // So, we´re cheating... "borrowing" a payment guid (saves time, but would like to have a "real type" & a guid)
            PaymentMethodDto payDto = PaymentManager.GetPaymentMethodBySystemName
                ("Generic", ContentLanguage.PreferredCulture.Name);

            giftCardPayment.PaymentMethodId = payDto.PaymentMethod[0].PaymentMethodId;
            // ...allmost okay for this demo/lab

            giftCardPayment.TransactionType = TransactionType.Sale.ToString();

            // could wrap a lot of what´s done here in a transaction scope...
            cart.GetFirstForm().Payments.Add(giftCardPayment);

            // ToDo: Maybe add identification of the card in the Order 

            // Can have more finesse here, the exercise just buys for more than the card balance...
            // Reset and deactivate the giftcard if it´s now empty (in the exercise it is)
            this.TheGiftCard["IsActive"] = false;
            this.TheGiftCard["Balance"] = 0M;

            // persist the card new details
            BusinessManager.Update(this.TheGiftCard);

            giftCardPayment.Status = PaymentStatus.Processed.ToString();
        }
        #endregion

        #region GiftCard and ClubCard
        public bool CheckForGiftCard(out Money GiftCardAmount)
        {
            // Check if logged in (and have a GiftCard)
            if (CustomerContext.Current.CurrentContact != null) // else anonymous
            {
                this.TheGiftCard = GetTheGiftCard(); // 

                GiftCardAmount = new Money(0, _currentMarket.Service.GetCurrentMarket().DefaultCurrency);

                if (this.TheGiftCard != null) // ...a card exist
                {
                    if (bool.Parse(TheGiftCard["IsActive"].ToString()) == true & (decimal)TheGiftCard["Balance"] >= 1)
                    {
                        FrontEndMessage = "Gift Card found, do you wish to use it? Balance = " + TheGiftCard["Balance"].ToString();
                        GiftCardAmount = new Money((decimal)TheGiftCard["Balance"], new Currency(_currentMarket.Service.GetCurrentMarket().DefaultCurrency));
                    }
                    else
                    {
                        FrontEndMessage = "...no active Gift Cards";
                        return false;
                    }
                }
                return true;
            }
            else // CurrentContact == null ...prompt for joining the "club"
            {
                FrontEndMessage = "Join our loyalty program and recieve points...:)";
                GiftCardAmount = new Money(0, _currentMarket.Service.GetCurrentMarket().DefaultCurrency);
                return false;
            }
        }

        public EntityObject GetTheGiftCard()
        {
            return BusinessManager.List
                    ("TrainingGiftCard", new[] {
                        FilterElement.EqualElement("ContactId"
                                , CustomerContext.Current.CurrentContactId)
                    }, null) // 
                    .FirstOrDefault(); // demo/lab ... could change the sort-null to a "new empty", both cannot be null
        }

        public void UpdateClubCard(ICart theCart, decimal totalSpent)
        {
            // RoCe: tot comes in subtracted, don't need to do it in here
            // only one club-card per buyer in this store
            PrimaryKeyId pk = (PrimaryKeyId)CustomerContext.Current.CurrentContactId;

            EntityObject[] theCards = BusinessManager.List
                ("ClubCard", new[] { FilterElement.EqualElement("ReferenceFieldNameId", pk) });

            if (theCards.Count() != 0) // ...is there one or more card(s))
            {
                // grab the first one as demo
                int balance = (int)theCards[0]["Balance"]; // Collected purchase-points

                // Get cardtype for current cust for calc of points (Gold, Silver or Bronze)
                int value = (int)theCards[0]["CardTypeEnum"]; // gets the int

                // probably some store policy involved... instead of the silly example below
                int newBalance = balance + (int)Math.Round(totalSpent / value); // this is a demo :)
                theCards[0]["Balance"] = newBalance;
                BusinessManager.Update(theCards[0]);
            }
        }

        #endregion





    }
}
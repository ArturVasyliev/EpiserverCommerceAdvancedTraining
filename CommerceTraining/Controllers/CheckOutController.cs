using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Engine;
using System;
using EPiServer.Security;
using Mediachase.Commerce.Customers;
using EPiServer.ServiceLocation;
using EPiServer.Globalization;
using System.Globalization;
using Mediachase.Commerce.Core;
using CommerceTraining.SupportingClasses;
using EPiServer.Find;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.BusinessFoundation.Data;
using System.Messaging;
using Mediachase.Commerce.Orders.Exceptions;
using CommerceTraining.Infrastructure.CartAndCheckout;
using EPiServer.Commerce.Marketing;
using CommerceTraining.Models.Promotions;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Markets;
//using CommerceTraining.Infrastructure.Shipping;
//using Mediachase.FileUploader.Configuration;

namespace CommerceTraining.Controllers
{
    public class CheckOutController : OrderControllerBase<CheckOutPage>
    {
        #region Properties added for Adv.

        // added for Adv.
        private string AgreeSplitShip { get; set; } // not used right now
        private static bool AddSecondShipment { get; set; }
        private static bool IsOnLine { get; set; }

        // for test both to true if logged in
        //bool useGiftCard = false; // toggle to true for test
        //bool addSecondPayment = false; // initially...

        protected string frontEndMessage = String.Empty; // messages from BF-GiftCard
        protected bool chkGiftCard = false;
        protected bool poToQueue = false; // toggle for now...

        #endregion

        // kind of helper/clean up here and add stuff
        CartAndCheckoutService ccService = new CartAndCheckoutService(); 

        public CheckOutController(
              IOrderRepository orderRepository
            , IOrderGroupFactory orderGroupFactory // RoCe: change
            , IOrderGroupCalculator orderGroupCalculator
            , IContentLoader contentLoader
            , ILineItemCalculator lineItemCalculator
            , IPlacedPriceProcessor placedPriceProcessor
            , IInventoryProcessor inventoryProcessor
            , ILineItemValidator lineItemValidator
            , IPromotionEngine promotionEngine
            , ICurrentMarket currentMarket
            , IPaymentProcessor paymentProcessor)
            : base(
                orderRepository, orderGroupFactory, orderGroupCalculator, contentLoader
                , lineItemCalculator, placedPriceProcessor, inventoryProcessor
                , lineItemValidator, promotionEngine, currentMarket, paymentProcessor)
        {

        }

        public ActionResult Index(CheckOutPage currentPage, bool passedAlong) 
        {
            IsOnLine = CheckIfOnLine.IsInternetAvailable; // for Find ... if on the bus

            // Load the cart, it should be one there
            ICart cart = base.GetCart();
            if (cart == null)
            {
                throw new InvalidOperationException("No cart found"); // make nicer
            }
            
            var model = new CheckOutViewModel(currentPage)
            {
                // ToDo: get shipments & payments
                PaymentMethods = GetPaymentMethods(),
                ShipmentMethods = GetShipmentMethods(),
                ShippingRates = GetShippingRates(),
                ShippingMethodInfo = CheckIfShippingMethodIsOkay() // not much done in this one, yet
            };

            return View(model);
        }

        #region From Fund

        private IEnumerable<ShippingRate> GetShippingRates()
        {
            List<ShippingRate> shippingRates = new List<ShippingRate>();
            IEnumerable<ShippingMethodDto.ShippingMethodRow> shippingMethods = GetShipmentMethods();

            foreach (var item in shippingMethods)
            {
                shippingRates.Add(new ShippingRate(item.ShippingMethodId, item.DisplayName
                    , new Money(item.BasePrice, _currentMarket.GetCurrentMarket()
                    .DefaultCurrency)
                    .Round()));
            }

            return shippingRates;
        }

        private IEnumerable<PaymentMethodDto.PaymentMethodRow> GetPaymentMethods()
        {
            string lang = ContentLanguage.PreferredCulture.TwoLetterISOLanguageName;
            return new List<PaymentMethodDto.PaymentMethodRow>(
                  PaymentManager
                  .GetPaymentMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value)
                  .PaymentMethod);
        }

        private IEnumerable<ShippingMethodDto.ShippingMethodRow> GetShipmentMethods()
        {
            IMarket market = _currentMarket.GetCurrentMarket();
            var str = market.DefaultLanguage.TwoLetterISOLanguageName;

            return new List<ShippingMethodDto.ShippingMethodRow>(
                ShippingManager
                .GetShippingMethodsByMarket(market.MarketId.Value, false)
                .ShippingMethod);
        }

        #endregion

        private string CheckIfShippingMethodIsOkay()
        {
            // ...could use this one for ship-info, 
            // would like a check ShippingOptionParameters and other stuff here
            return "Check on shipments... "; // ...for now
        }

        // This method is about what we ended up with in "Fund." - with a few changes done for Adv.
        public ActionResult CheckOut(CheckOutViewModel model)
        {
            // SplitPay is in a Session-variable (bool)
            string paymentProcessResult = String.Empty;

            // Load the cart, it should be one there
            var cart = _orderRepository.Load<ICart>(GetContactId(), "Default").FirstOrDefault();
            if (cart == null)
            {
                throw new InvalidOperationException("No cart found"); // make nicer
            }

            // From Fund
            IOrderAddress theAddress = AddAddressToOrder(cart);

            // ToDo: Added this field for Adv. & Find ... doing it simple now using one Address 
            // The address is for Find, but we need to add it to MDP to be able to use it properly
            // This is a Serialized cart, so doesn't crash if the field is not added to MDP 
            theAddress.Properties["AddressType"] = "Shipping";

            #region Ship & Pay from Fund

            // ToDo: Define Shipping - From Fund
            AdjustFirstShipmentInOrder(cart, theAddress, model.SelectedShipId); // ...as a Shipment is added by epi 

            // ToDo: Define Payment - From Fund
            AddPaymentToOrder(cart, model.SelectedPayId); // ...as this is not added by default

            #endregion

            #region Split Pay

            // RoCe: Fix this - addSecondPayment comes in as a param (bool) 
            // ... force for now if BF-Card is found ... using Session
            if ((bool)Session["SecondPayment"] == true)
            {
                ccService.AddSecondPaymentToOrder(cart);
            }

            // gathered info
            this.frontEndMessage = ccService.FrontEndMessage;

            #endregion

            // Possible change of the cart... adding this 
            // would have this done if a flag were set
            var cartReference = _orderRepository.Save(cart);

            // Original Fund... (with additions)
            IPurchaseOrder purchaseOrder;
            OrderReference orderReference;

            #region Transaction Scope

            using (var scope = new Mediachase.Data.Provider.TransactionScope()) // one in BF, also
            {
                var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

                // Added - sets a lock on inventory... 
                // ...could come earlier (outside tran) depending on TypeOf-"store"
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                    , OrderStatus.InProgress, (item, issue) => validationIssues.Add(item, issue));

                if (validationIssues.Count >= 1)
                {
                    throw new Exception("Not possible right now"); // ...change approach
                }

                // just checking the cart in watch window
                var theShipping = cart.GetFirstShipment();
                var theLineItems = cart.GetAllLineItems();
                var firstPayment = cart.GetFirstForm().Payments.First(); // no "GetFirstPayment()"
                var theforms = cart.Forms;
                
                PaymentProcessingResult otherResult =
                _paymentProcessor.ProcessPayment(cart, cart.GetFirstForm().Payments.First(), cart.GetFirstShipment());

                frontEndMessage += otherResult.Message;

                if (otherResult.IsSuccessful)
                {
                    IPayment thePay = cart.GetFirstForm().Payments.First();
                    thePay.Status = PaymentStatus.Processed.ToString();
                }
                else
                {
                    IPayment thePay = cart.GetFirstForm().Payments.First();
                    thePay.Status = PaymentStatus.Failed.ToString();
                    throw new System.Exception("Bad payment"); // could have more grace
                }

                // ...only one form, still
                var totalProcessedAmount = cart.GetFirstForm().Payments.Where
                    (x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);

                // nice extension method
                var cartTotal = cart.GetTotal();

                // Do inventory - decrement or put back in stock
                if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    // put back the reserved request
                    _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                        , OrderStatus.Cancelled, (item, issue) => validationIssues.Add(item, issue));

                    throw new InvalidOperationException("Wrong amount"); // maybe change approach
                }

                // RoCe: have to do Promos here also ... move stuff from cart to "base"
                // simulation... should be an "else"
                cart.GetFirstShipment().OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;
                
                // decrement inventory and let it go
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                    , OrderStatus.Completed, (item, issue) => validationIssues.Add(item, issue));

                // Should do the ClubCard thing here - ClubMembers are logged in
                // PaymentMethodName = "GiftCard"
                if (CustomerContext.Current.CurrentContact != null)
                {
                    // check if GiftCard was used, don't give bonus for that payment
                    IEnumerable<IPayment> giftCardPayment = cart.GetFirstForm().Payments.Where
                        (x => x.PaymentMethodName.Equals("GiftCard"));

                    if (giftCardPayment.Count() >= 1)
                    {
                        ccService.UpdateClubCard(cart, totalProcessedAmount - giftCardPayment.First().Amount);
                    }
                    else
                    {
                        // no GiftCard, but collecting points
                        ccService.UpdateClubCard(cart, totalProcessedAmount);
                    }
                }

                orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                _orderRepository.Delete(cart.OrderLink);

                scope.Complete();
            } // End Tran

            #endregion
            
            // just demoing (Find using this further down)
            purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);

            // check the below
            var theType = purchaseOrder.OrderLink.OrderType;
            var toString = purchaseOrder.OrderLink.ToString(); // Gets ID and Type ... combined

            #region ThisAndThat - from Fund

            OrderStatus poStatus;
            poStatus = purchaseOrder.OrderStatus;
            //purchaseOrder.OrderStatus = OrderStatus.InProgress;

            //var info = OrderStatusManager.GetPurchaseOrderStatus(PO);

            var shipment = purchaseOrder.GetFirstShipment();
            var status = shipment.OrderShipmentStatus;

            //shipment. ... no that much to do
            shipment.OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;
            
            var notes = purchaseOrder.Notes; // IOrderNote is 0
            
            // have getters & setters... not good
            Mediachase.Commerce.Orders.OrderNote otherNote = new OrderNote //IOrderNote 
            {
                // Created = DateTime.Now, // do we need to set this ?? Nope .ctor does
                CustomerId = new Guid(), // can set this - regarded
                Detail = "Order ToString(): " + toString + " - Shipment tracking number: " + shipment.ShipmentTrackingNumber,
                LineItemId = purchaseOrder.GetAllLineItems().First().LineItemId,
                // OrderGroupId = 12, R/O - error
                // OrderNoteId = 12, // can define it, but it's disregarded - no error
                Title = "Some title",
                Type = OrderNoteTypes.Custom.ToString()
            }; // bug issued

            purchaseOrder.Notes.Add(otherNote); // void back
            purchaseOrder.ExpirationDate = DateTime.Now.AddMonths(1);

            // yes, still need to come after adding notes
            _orderRepository.Save(purchaseOrder); // checking down here ... yes it needs to be saved again

            #endregion

            string conLang0 = ContentLanguage.PreferredCulture.Name;

            // original shipment, could rewrite and get the dto so it can be used for the second shipment also
            // or grab the dto when loading into the dropdowns
            ShippingMethodDto.ShippingMethodRow theShip =
                ShippingManager.GetShippingMethod(model.SelectedShipId).ShippingMethod.First();

            #region Find & Queue plumbing

            // would be done async...
            if (IsOnLine) // just checking if the below is possible, if we have network access
            {
                // index PO and addresses for BoughtThisBoughtThat & demographic analysis
                IClient client = Client.CreateFromConfig(); // native
                FindQueries Qs = new FindQueries(client, true);
                Qs.OrderForFind(purchaseOrder);
            }

            if (poToQueue) // could have better tran-integrity, Extraction later in PO_Extract.sln/Sheduled job
            {
                // ToDo: Put a small portion of data from the PO to msmq, will eventually (out-of-process) go to the ERP
                string QueueName = ".\\Private$\\MyQueue";
                MessageQueue Q1 = new MessageQueue(QueueName);
                MyMessage m = new MyMessage()
                {
                    poNr = purchaseOrder.OrderNumber,
                    status = purchaseOrder.OrderStatus.ToString(),
                    orderGroupId = orderReference.OrderGroupId
                };

                Q1.Send(m);
            }

            #endregion

            // Final steps, navigate to the order confirmation page
            StartPage home = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            ContentReference orderPageReference = home.Settings.orderPage;

            string passingValue = frontEndMessage + paymentProcessResult + " - " + purchaseOrder.OrderNumber;
            return RedirectToAction("Index", new { node = orderPageReference, passedAlong = passingValue });
        }

        #region From Fund. Address & Shipping

        private IOrderAddress AddAddressToOrder(ICart cart)
        {
            IOrderAddress shippingAddress;

            if (CustomerContext.Current.CurrentContact == null)
            {
                // Anonymous... one way of "doing it"... for example, if no other address exist
                var shipment = cart.GetFirstShipment(); // ... moved to shipment - prev. = .OrderAddresses.Add(

                if (shipment.ShippingAddress != null)
                {
                    //return false/true; // Should clean up? 
                }


                //Shipment oldShip = shipment as Shipment;
                shippingAddress = shipment.ShippingAddress = // should be an else here... below?
                    new OrderAddress
                    {
                        CountryCode = "USA",
                        CountryName = "United States",
                        Name = "SomeCustomerAddressName",
                        DaytimePhoneNumber = "123456",
                        FirstName = "John",
                        LastName = "Smith",
                        Email = "John@company.com",
                    };

            }
            else
            {
                // Logged in
                if (CustomerContext.Current.CurrentContact.PreferredShippingAddress == null)
                {
                    // no pref. address set... so we set one for the contact
                    CustomerAddress newCustAddress =
                        CustomerAddress.CreateForApplication();
                    newCustAddress.AddressType = CustomerAddressTypeEnum.Shipping; // mandatory
                    newCustAddress.ContactId = CustomerContext.Current.CurrentContact.PrimaryKeyId;
                    newCustAddress.CountryCode = "SWE";
                    newCustAddress.CountryName = "Sweden";
                    newCustAddress.Name = "new customer address"; // mandatory
                    newCustAddress.DaytimePhoneNumber = "123456";
                    newCustAddress.FirstName = CustomerContext.Current.CurrentContact.FirstName;
                    newCustAddress.LastName = CustomerContext.Current.CurrentContact.LastName;
                    newCustAddress.Email = "GuitarWorld@Thule.com";

                    // note: Line1 & City is what is shown in CM at a few places... not the Name
                    CustomerContext.Current.CurrentContact.AddContactAddress(newCustAddress);
                    CustomerContext.Current.CurrentContact.SaveChanges();

                    // ... needs to be in this order
                    CustomerContext.Current.CurrentContact.PreferredShippingAddress = newCustAddress;
                    CustomerContext.Current.CurrentContact.SaveChanges(); // need this ...again 

                    // then, for the cart
                    shippingAddress = new OrderAddress(newCustAddress); // - NEW
                }
                else
                {
                    // 3:rd vay there is a preferred address set 
                    shippingAddress = new OrderAddress(
                        CustomerContext.Current.CurrentContact.PreferredShippingAddress);
                }
            }

            return shippingAddress;
        }

        private void AdjustFirstShipmentInOrder(ICart cart, IOrderAddress orderAddress, Guid selectedShip)
        {
            // Need to set the guid (name is good to have too) of some "real shipmentment in the DB"
            // RoCe - this step is not needed, actually - code and lab-steps can be updated
            // We'll do it to show how it works
            var shippingMethod = ShippingManager.GetShippingMethod(selectedShip).ShippingMethod.First();

            IShipment theShip = cart.GetFirstShipment(); // ...as we get one "for free"

            // Need the choice of shipment from DropDowns
            theShip.ShippingMethodId = shippingMethod.ShippingMethodId;
            //theShip.ShippingMethodName = "TucTuc";

            theShip.ShippingAddress = orderAddress;

            #region Hard coded and cheating just to show

            // RoCe: - fix the MarketService
            var mSrv = ServiceLocator.Current.GetInstance<IMarketService>();
            var defaultMarket = mSrv.GetMarket(MarketId.Default); // cheating some
            Money cost00 = theShip.GetShippingCost(_currentMarket.GetCurrentMarket(), new Currency("USD"));
            Money cost000 = theShip.GetShippingCost(_currentMarket.GetCurrentMarket(), cart.Currency);
            #endregion

            Money cost0 = theShip.GetShippingCost(
                _currentMarket.GetCurrentMarket()
                , _currentMarket.GetCurrentMarket().DefaultCurrency); // to make it easy

            // done by the "default calculator"
            Money cost1 = theShip.GetShippingItemsTotal(_currentMarket.GetCurrentMarket().DefaultCurrency);

            theShip.ShipmentTrackingNumber = "ABC123";
        }

        // RoCe - this can also be simplified
        private void AddPaymentToOrder(ICart cart, Guid selectedPaymentGuid)
        {
            if (cart.GetFirstForm().Payments.Any())
            {
                // should maybe clean up in the cart here
            }

            var selectedPaymentMethod =
                PaymentManager.GetPaymentMethod(selectedPaymentGuid).PaymentMethod.First();

            var payment = _orderGroupFactory.CreatePayment(cart);

            payment.PaymentMethodId = selectedPaymentMethod.PaymentMethodId;
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodName = selectedPaymentMethod.Name; 

            payment.Amount = _orderGroupCalculator.GetTotal(cart).Amount; 

            cart.AddPayment(payment);
            // could add payment.BillingAddress = theAddress ... if we had it here
        }

        #endregion

    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Engine;
using CommerceTraining.Models.ViewModels;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using System;
using Mediachase.Commerce.Orders.Dto;
using CommerceTraining.Infrastructure.CartAndCheckout;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;

namespace CommerceTraining.Controllers
{
    public class CartController : OrderControllerBase<CartPage>
    {
        public static IContentLoader _contentLoader2;
        
        public CartController(
              IOrderRepository orderRepository
            , IOrderGroupFactory orderGroupFactory
            , IOrderGroupCalculator orderGroupCalculator
            , IContentLoader contentLoader
            , ILineItemCalculator lineItemCalculator
            , IPlacedPriceProcessor placedPriceProcessor
            , IInventoryProcessor inventoryProcessor
            , ILineItemValidator lineItemValidator
            , IPromotionEngine promotionEngine
            , ICurrentMarket currentMarket
            , IPaymentProcessor paymentProcessor
            ) : base
            (orderRepository, orderGroupFactory, orderGroupCalculator, contentLoader
                , lineItemCalculator, placedPriceProcessor, inventoryProcessor
                , lineItemValidator, promotionEngine, currentMarket, paymentProcessor) // Should maybe not have the last one here
        {
            _contentLoader2 = contentLoader;
        }

        // ...kind of helper/"service"
        CartAndCheckoutService ccService = new CartAndCheckoutService();

        // for various messages going to front-end
        List<string> otherMessages = new List<string>();

        // Does: GetCart --> ValidateCart --> CleanUpCart --> CheckShipping - 
        // --> Calculate: Totals & Promos --> SaveCart

        Injected<ITaxCalculator> _taxCalc;

        public ActionResult Index(CartPage currentPage, string routedData)
        {
            var cart = base.GetCart(); // "LoadOrCreate" is done in "VariationController"

            if (cart == null)
            {
                return View("NoCart"); // can improve this
            }
            else
            {
                var warningMessages = base.ValidateCart(cart);

                // RoCe: improve the message part
                otherMessages.Add(ccService.FrontEndMessage); // frontEndMessage is about the GiftCard

                //if ((bool)cart.Properties["SpecialShip"]) // SpecialShip is set in Trousers-Controller
                if(ccService.CheckOnLineItems(cart))
                {
                    otherMessages.Add("Note that one or more item(s) require separate shipment");
                    otherMessages.Add(CheckShipping(cart)); // starting to investigate restrictions for SKU-shipping
                }
                else
                {
                    otherMessages.Clear();
                }

                //RoCe: ... need to check for GiftCards
                Money giftCardAmount = new Money(0, _currentMarket.GetCurrentMarket().DefaultCurrency);
                if (ccService.CheckForGiftCard(out giftCardAmount)) // maybe don't need the "out-part"
                {
                    // Get a message out to Front-End
                    otherMessages.Add(ccService.FrontEndMessage);
                    Session["SecondPayment"] = true; // improve
                }
                else
                {
                    Session["SecondPayment"] = false; // improve
                }

                string messageString = String.Empty;
                foreach (var item in otherMessages)
                {
                    messageString += item;
                }

                if (String.IsNullOrEmpty(warningMessages))
                {
                    warningMessages += "No error messages - ";
                }

                IEnumerable<RewardDescription> discounts = cart.ApplyDiscounts();
                foreach (var item in discounts)
                {
                    messageString += "Saved amount" + item.SavedAmount;
                }

                // just checking
                var validationIssues = new Dictionary<ILineItem, ValidationIssue>();
                foreach (var item in cart.GetAllLineItems())
                {
                    _placedPriceProcessor.UpdatePlacedPrice(
                        item,GetContact(),_currentMarket.GetCurrentMarket().MarketId
                        ,cart.Currency,
                    ((item2, issue) => validationIssues.Add(item, issue)));

                    var p = _lineItemCalculator.GetDiscountedPrice(item, cart.Currency);
                 }

                var tot = _orderGroupCalculator.GetSubTotal(cart);
                var disc = _orderGroupCalculator.GetOrderDiscountTotal(cart);

                var model = new CartViewModel
                {
                    LineItems = cart.GetAllLineItems(), // Extension method
                    CartTotal = _orderGroupCalculator.GetSubTotal(cart) - _orderGroupCalculator.GetOrderDiscountTotal(cart),
                    Messages = warningMessages + " - " + messageString,// + CheckShipping(cart), // added the method
                    PromotionMessages = base.GetPromotions(cart)
                };

                _orderRepository.Save(cart);

                return View("index", model);
            }
        }

        #region Move To Base or "service" when time permits

        // ...if so, call for what ShippingMethods is/are applicable
        private string CheckShipping(ICart cart)
        {
            // ...just using the Entry code... as it's set in the Lineitem
            string str = String.Empty;
            var lineItems = cart.GetAllLineItems();

            foreach (ILineItem item in lineItems)
            {
                if ((bool)item.Properties["LineItemSpecialShipping"])
                {
                    // look for ShippingMethodParameters that match (the method below)
                    str += item.Code + ": will ship by: " + CheckApplicableShippings(item.Code);
                }
            }
            return str;
        }

        private string CheckApplicableShippings(string code)
        {
            /* load the Shipping-Method-Parameters and inspect what method will be used... 
             * for now, just output text to the page, could also use it for loading "the right" method-guid
             *   for the second shipment added */
            string str = String.Empty;
            
            // get the rows, we don't care about the market-part now
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket
                (MarketId.Default.Value, false);

            foreach (ShippingMethodDto.ShippingMethodRow shippingMethod in dto.ShippingMethod)
            {
                // This is the interesting part, could be complemented with dbo.ShippingOptionParameter
                // get the param-rows, could show the dbo.ShippingMethodParameter table in VS
                ShippingMethodDto.ShippingMethodParameterRow[] paramRows =
                    shippingMethod.GetShippingMethodParameterRows();
                
                if (paramRows.Count() != 0)
                {
                    // right now we're just outputting text to the cart
                    //   ... should more likely get the guid instead
                    // could be here we can match lineItem with the shipping method or gateway
                    foreach (var shippingMethodParameter in paramRows) 
                    {
                        str += shippingMethod.Name + " : " + shippingMethodParameter.Parameter + " or ";
                        var v = shippingMethodParameter.Value; // ...not in use... yet...
                    }
                }
            }
            return str;
        }

        #endregion

        // Go to CheckOut
        public ActionResult Checkout(CartPage currentPage, string agreeSplitPayment) // chk gives "" or null
        {

            // Final steps and go to checkout
            StartPage home = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            ContentReference theRef = home.Settings.checkoutPage;

            bool passingValue; // could turn this into split pay
            if (agreeSplitPayment == null)
            {
                passingValue = false;
            }
            else
            {
                passingValue = true;
            }

            return RedirectToAction("Index", new { node = theRef, passedAlong = passingValue });
        }
    }
}
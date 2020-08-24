using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce.Customers;
using System;
using EPiServer.Security;
using Mediachase.Commerce.Security; // For PrincipalInfo.CurrentPrincipal.GetContactId();
using Mediachase.Commerce;
using CommerceTraining.Models.Catalog;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;

namespace CommerceTraining.Controllers
{
    public class OrderControllerBase<T> : ContentController<T> where T : PageData
    {
        private const string DefaultCartName = "Default";

        public readonly IOrderRepository _orderRepository;
        public readonly IOrderGroupFactory _orderGroupFactory;
        public readonly IOrderGroupCalculator _orderGroupCalculator;
        public readonly IPromotionEngine _promotionEngine;
        public readonly IContentLoader _contentLoader;
        public readonly ILineItemCalculator _lineItemCalculator;
        public readonly IInventoryProcessor _inventoryProcessor;
        public readonly ILineItemValidator _lineItemValidator;
        public readonly IPlacedPriceProcessor _placedPriceProcessor;
        public readonly ICurrentMarket _currentMarket;
        public readonly IPaymentProcessor _paymentProcessor;


        public OrderControllerBase(
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
            , IPaymentProcessor paymentProcessor)
        {
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _contentLoader = contentLoader;
            _promotionEngine = promotionEngine;
            _lineItemCalculator = lineItemCalculator;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _currentMarket = currentMarket;
            _paymentProcessor = paymentProcessor;
        }

        public ICart GetCart()
        {
            ICart cart;

            // LoadOrCreate done in VariationController, should be a cart in DB
            cart = _orderRepository.LoadCart<ICart>(
                GetContactId()
                , DefaultCartName);

            // Might want to clean-up among addresses, payments and Shipments
            // If the payment step failed in the CheckOutController the IPayment lingers in the cart
            // ...my example for "PayMe"

            List<IPayment> paymentList = new List<IPayment>();

            if (paymentList.Count != 0) // quick fix, for now
            {
                foreach (IOrderForm item in cart.Forms)
                {
                    foreach (IPayment p in item.Payments)
                    {
                        paymentList.Add(p);
                    }
                }

                foreach (var p in paymentList)
                {
                    foreach (IOrderForm item in cart.Forms)
                    {
                        item.Payments.Remove(p);
                    }
                }
            }

            return cart;
        }

        public string GetPromotions(ICart cart)
        {
            // ...very simple, mostly for verification and/or maybe do something 
            // depending on the descriptions we get back
            String str = String.Empty;

            var rewardDescriptions = _promotionEngine.Run(cart).ToList();
            rewardDescriptions.ForEach(r => str += r.Description);

            #region just checking, not doing anything with this

            IEnumerable<RedemptionDescription> redemptions;
            RedemptionLimitsData red;

            foreach (var item in cart.GetAllLineItems())
            {
                decimal d = item.GetEntryDiscount();
                Money m = item.GetDiscountedPrice(cart.Currency);
            }

            foreach (var item in rewardDescriptions)
            {
                red = item.Promotion.RedemptionLimits;
                redemptions = item.Redemptions;
            }

            #endregion

            // example with Coupons in QS

            return str;
        }

        // ...from Fund.
        public string ValidateCart(ICart cart)
        {
            var validationMessages = string.Empty;
            List<ValidationIssue> theList = new List<ValidationIssue>(); // could have a look at the enum in reflector

            cart.ValidateOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _lineItemValidator);

            cart.UpdatePlacedPriceOrRemoveLineItems(GetContact(), (item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _placedPriceProcessor);

            // need to keep an eye on this during CheckOut, not doing an inventory-request here 
            //   ...just doing a check
            cart.UpdateInventoryOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue)
                , _inventoryProcessor);

            // if it fit in, we could do... cart.ApplyDiscounts() ... have a few overloads we could use

            return validationMessages;
        }

        public static string CreateValidationMessages(ILineItem item, ValidationIssue issue)
        {
            return string.Format("Line item with code {0} had the validation issue {1}.", item.Code, issue);
        }

        protected static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        protected static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }

        #region Just checking on WH
        
        Injected<IWarehouseRepository> warehouseRepository;
        public string GetClosestWareHouse(DefaultVariation sku)
        {
            string str = String.Empty;

            var m = _currentMarket.GetCurrentMarket().MarketId.Value;
            var w = warehouseRepository.Service.List();
            foreach (var item in w)
            {
                if (item.Code == m)
                {
                    str = item.Name;
                    break;
                }
                else
                {
                    str = "...nothing close";
                }
            }

            return str;
        }

        #endregion
    }
}
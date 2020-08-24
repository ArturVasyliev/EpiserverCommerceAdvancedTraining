using CommerceTraining.Models.Promotions;
using EPiServer;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using EPiServer.Commerce.Marketing.Extensions;
using EPiServer.Framework.Localization; // note this one
using EPiServer.Commerce.Extensions;
using Mediachase.Commerce;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace CommerceTraining.Infrastructure.Promotions
{
    // ... a promotion processor for "MyPercentagePromotion"
    //[ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class MyPercentagePromotionProcessor : EntryPromotionProcessorBase<MyPercentagePromotion>
    {
        /* The processor evaluates if a promotion should apply a reward to an order/Entry. 
         *  ...can implement the IPromotionProcessor interface directly, 
         *  ...but it's recommended to inherit from the abstract ones below
         *   - EntryPromotionProcessorBase<TEntryPromotion>, 
         *   - OrderPromotionProcessorBase<TOrderPromotion>, 
         *   - ShippingPromotionProcessorBase<TShippingPromotion> 
        */

        private readonly CollectionTargetEvaluator _targetEvaluator;
        private readonly IContentLoader _contentLoader;
        private readonly FulfillmentEvaluator _fulfillmentEvaluator;
        private readonly LocalizationService _localizationService;

        public MyPercentagePromotionProcessor(
            CollectionTargetEvaluator targetEvaluator,
            FulfillmentEvaluator fulfillmentEvaluator,
            LocalizationService localizationService,
            IContentLoader contentLoader,
            RedemptionDescriptionFactory redemptionDescriptionFactory)
            :base(redemptionDescriptionFactory)
        {
            _contentLoader = contentLoader;
            _targetEvaluator = targetEvaluator;
            _fulfillmentEvaluator = fulfillmentEvaluator;
            _localizationService = localizationService;
        }


        /* RewardDescription is "checking" whether the promotion was fulfilled (or not, or partially),
            ...which items the promotion was applied to... 
            ...the fake-cart is taken care of)
           PromotionProcessorBase has one abstract method to be implemented, Evaluate. 
           ...the method is supplied with a PromotionData, and a PromotionProcessorContext object 
           ...that contains information about the current order/fakeCart. 
         */

        protected override RewardDescription Evaluate(// note: it's OrderForm now... 
             MyPercentagePromotion promotionData // the model --> look in the UI to see the properties
            , PromotionProcessorContext context)
        {
            /* A reward description contains information about if and how a reward is applied. 
             * ...some properties are:
             *   - A list of redemption descriptions, one for each of the maximum amount of redemptions 
             *      ...that could be applied to the current order. 
             *     This does not have to take redemption limits into consideration, that is handled by the 
             *      promotion engine.
             *   - A reward type. Depending on the type, the promotion value is read from the properties 
             *      UnitDiscount, Percentage or Quantity.
             *   - A status flag. Indicates if a promotion is not, partially, or fully fulfilled.
             *   - A saved amount. The amount by which this reward reduces the order cost. 
             *      Is set by the promotion engine; should not be set in the promotion processor*/

            IOrderForm orderForm = context.OrderForm; // OrderForm now pops in with the context 
            //context. // lots of things
            IEnumerable<ILineItem> lineItemsCheck = orderForm.GetAllLineItems();
            IEnumerable<ILineItem> lineItems = GetLineItems(context.OrderForm);

            #region Just Checking

            //var e = _contentLoader.Get<EntryContentBase>(item0); 

            //should check if it's applicable... at all

            //var li = _orderFactory.Service.CreateLineItem(e.Code);
            //li.Quantity = 1;
            //li.PlacedPrice = 15;
            //orderForm.Shipments.First().LineItems.Add(li);

            #endregion

            // GetFulfillmentStatus - extension method
            FulfillmentStatus status = promotionData.MinNumberOfItems.GetFulfillmentStatus(
                orderForm, _targetEvaluator, _fulfillmentEvaluator);

            List<RewardDescription> rewardDescriptions = new List<RewardDescription>();
            List<RedemptionDescription> redemptionDescriptions = new List<RedemptionDescription>();

            #region NewStuff

            // The below does not see the cart, it's for landing pages for the Promotion itself (rendering)
            PromotionItems promoItems = GetPromotionItems(promotionData); // gets null

            var condition = promotionData.PercentageDiscount; // ...in the model
            var targets = promotionData.DiscountTargets; // get one in any case, points to what's at "promo"

            var skuCodes = _targetEvaluator.GetApplicableCodes(
                lineItems, targets.Items, targets.MatchRecursive); // get one if kicked in, 0 if not

            var fulfillmentStatus = _fulfillmentEvaluator.GetStatusForBuyQuantityPromotion(
                skuCodes
                , lineItems
                , promotionData.MinNumberOfItems.RequiredQuantity
                , promotionData.PartialFulfillmentNumberOfItems);

            // Just checking
            // The promotion engine creates a "price matrix" for all items in the order form.
            // OrderFormPriceMatrix, is accessible through the EntryPrices property 
            //   of the PromotionProcessorContext object. 
            // PromotionProcessorContext is passed to the Evaluate method as one of the arguments.
            //  ...the matrix holds "codes" and quantity  
            // The second ExtractEntries call starts to receive entries where the first call ended. 
            //   ... makes it easy to create several redemptions by calling ExtractEntries in a loop, 
            //   ... and create one RedemptionDescription inside the loop.
            // The price matrix has one public method (ExtractEntries)
            //   ... two overloads, both overloads takes entry codes and quantity as parameters. 
            //   ... one contains an action for getting the entries in a specific order. 
            //   ... if no specific order is specified, MostExpensiveFirst is used.
            var affectedEntries = context.EntryPrices.ExtractEntries(
                skuCodes,
                1); // get one if it kicks in, null if not

            if (affectedEntries != null)
            {
                IEnumerable<PriceEntry> priceEntries = affectedEntries.PriceEntries;
                foreach (var item in priceEntries)
                {
                    var qty = item.Quantity;
                    var price = item.Price;
                    var calc = item.CalculatedTotal; // involves the Qty
                    var actuals = item.ActualTotal; // includes rounding            
                }
            }

            // could have a look here
            switch (fulfillmentStatus)
            {
                case FulfillmentStatus.NotFulfilled:
                    break;
                case FulfillmentStatus.PartiallyFulfilled:
                    break;
                case FulfillmentStatus.Fulfilled:
                    break;
                case FulfillmentStatus.CouponCodeRequired:
                    break;
                case FulfillmentStatus.Excluded:
                    break;
                case FulfillmentStatus.VisitorGroupRequired:
                    break;
                case FulfillmentStatus.RedemptionLimitReached:
                    break;
                case FulfillmentStatus.NoMoneySaved:
                    break;
                case FulfillmentStatus.InvalidCoupon:
                    break;
                case FulfillmentStatus.InvalidCombination:
                    break;
                case FulfillmentStatus.MissingVisitorGroup:
                    break;
                case FulfillmentStatus.NoRedemptionRemaining:
                    break;
                case FulfillmentStatus.Ineffective:
                    break;
                default:
                    break;
            }

            // ... an extension method
            return RewardDescription.CreatePercentageReward(
                 fulfillmentStatus
                 , GetRedemptions(skuCodes, promotionData, context)
                 , promotionData
                 , promotionData.PercentageDiscount.Percentage
                 //, fulfillmentStatus.GetRewardDescriptionText()
                 , fulfillmentStatus.GetRewardDescriptionText() + " : " + promotionData.Description + " : "
                 );

            #endregion
            
            #region Older stuff and debug - no show

            #region Older not in use

            //RewardDescription rewardDescription = new RewardDescription();

            //var codes = _targetEvaluator.GetApplicableCodes(lineItems,)

            //_fulfillmentEvaluator.GetStatusForBuyQuantityPromotion(
            //    )

            #endregion // new stuff

            #region Previous version

            //if (status.HasFlag(FulfillmentStatus.Fulfilled))
            //{
            //    return RewardDescription.CreateMoneyOrPercentageRewardDescription(
            //        status,
            //        redemptionDescriptions,
            //        promotionData,
            //        promotionData.PercentageDiscount,
            //        context.OrderGroup.Currency,
            //        "Custom promotion fulfilled"); // should have a more flexible way... GetDescription()

            //}
            //else
            //{
            //    return RewardDescription.CreateNotFulfilledDescription(
            //        promotionData, FulfillmentStatus.NotFulfilled);
            //}


            #endregion

            #region Debug

            //RedemptionDescription rFirst;
            //redemptionDescriptions.Add(CreateRedemptionDescriptionText(orderForm));

            // below "if-construct" is for debug
            //if (promotionData.PercentageDiscount <= 0) // ... return "sorry, no discount"
            //{
            //    return RewardDescription.CreatePercentageReward(
            //        FulfillmentStatus.NotFulfilled,
            //        redemptionDescriptions,
            //        promotionData,
            //        0,
            //        CreateRewardDescriptionText(redemptionDescriptions.First(), FulfillmentStatus.NotFulfilled, promotionData));

            //    /*RewardDescription.CreateMoneyOrPercentageRewardDescription(FulfillmentStatus.NotFulfilled,r,promotionData,null);*/
            //}

            //IEnumerable<ContentReference> targetItems = promotionData.DiscountTargets.Items.ToList(); // set by the Promo-UI

            //bool matchRecursive = true; // walking down the catalog hierarchy
            //var lineItems = GetLineItems(orderForm); // "GetLineItems" - in the base class (PromotionProcessorBase)
            //var affectedItems = _targetEvaluator.GetApplicableItems(lineItems, targetItems, matchRecursive); // in CollectionTargetEvaluator
            //var affectedItems = _targetEvaluator.GetApplicableCodes(orderForm.GetAllLineItems(), targetItems, false);


            // small class --> just to get the status by the settings 
            //var status = FulfillmentEvaluator.GetStatusForBuyQuantityPromotion(affectedItems.Select(x => x.LineItem)
            //  , promotionData.MinNumberOfItems, promotionData.PartialFulfillmentNumberOfItems); // in the model
            //var s = FulfillmentEvaluator.

            //FulfillmentEvaluator ff = new FulfillmentEvaluator();

            //if (rewardDescriptions.Any())
            //{
            //    return rewardDescriptions.First();
            //}
            //else
            //{
            //    return null;
            //}


            /*return RewardDescription.CreateMoneyOrPercentageRewardDescription(
                status,
                affectedItems,
                promotionData,
                promotionData.PercentageDiscount,
                GetRewardDescriptionText(affectedItems, status, promotionData));*/

            #endregion

            #endregion

        } // end RewardDescription

        // used
        private IEnumerable<RedemptionDescription> GetRedemptions(
            IList<string> skuCodes
            , MyPercentagePromotion promotionData
            , PromotionProcessorContext context)
        {
            /* 
             * Primary goal is to identify the objects to which the redemption should apply. 
             * ... other than that, the RedemptionDescription also says how much this redemption saves on the 
             * ... order, and has a status flag that is set if the promotion engine (for some reason) 
             * ... decides not to apply this redemption. 
             * Depending on which type of promotion the reward gives (entry, order or shipping), 
             * ... different types of affected objects are used. 
             * ... to be found in either AffectedEntries, AffectedShipments or AffectedOrders. 
             * Use the CreateRedemptionDescription method on the promotion processor base classes 
             * ... to populate the redemption with the correct type of affected objects.*/

            var redemptions = new List<RedemptionDescription>();
            var requiredQuantity = promotionData.MinNumberOfItems;
            var maxRedemptions = GetMaxRedemptions(promotionData.RedemptionLimits);

            // have this one above also
            for (int i = 0; i < maxRedemptions; i++)
            {
                // ExtractEntries ... 3:rd argument is "sort", if not defined we get "most expensive" first
                // The ordering might be important, for example in the "Buy 3, get the cheapest for free".
                // the method sits on the PriceMatrix
                var affectedEntries = context.EntryPrices.ExtractEntries(
                    skuCodes
                    , 1);

                if (affectedEntries == null)
                {
                    break;
                }
                redemptions.Add(CreateRedemptionDescription(affectedEntries));
            }
            return redemptions;
        }

        // gets here ether way (kicks in or not)
        protected override PromotionItems GetPromotionItems(MyPercentagePromotion promotionData)
        {
            var targets = promotionData.DiscountTargets;
            var coupon = promotionData.Coupon.Code;
            var discountType = promotionData.DiscountType;
            var minNoOfItems = promotionData.MinNumberOfItems;
            var partialFullFilment = promotionData.PartialFulfillmentNumberOfItems;
            var redemptionLimits = promotionData.RedemptionLimits;
            var toString = promotionData.ToString(); // ...the Castle-proxy
            //throw new NotImplementedException();
            return null;
        }

        // used by promo-engine
        // ToDo: check this one
        protected override bool CanBeFulfilled(MyPercentagePromotion promotionData, PromotionProcessorContext context)
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            {
                return true;
            }
            else
            {
                //return (base.CanBeFulfilled(promotionData, context) && !ContentReference.IsNullOrEmpty(promotionData.Category));
                return false;
            }
            
        }

        // previously used, get it back in
        protected virtual string CreateCustomRewardDescriptionText(
            IEnumerable<RewardDescription> rewardDescriptions
            , FulfillmentStatus fulfillmentStatus
            , MyPercentagePromotion promotion
            , IOrderGroup orderGroup)
        {
            /*var contentNames = GetContentNames(affectedItems); // local method*/
            string contentNames = String.Empty;
            IEnumerable<ILineItem> lineItems = orderGroup.GetAllLineItems();

            if (fulfillmentStatus == FulfillmentStatus.Fulfilled)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}% discount has been given for the items '{1}'!"
                    , promotion.PercentageDiscount, contentNames);
            }

            if (fulfillmentStatus == FulfillmentStatus.PartiallyFulfilled) // have 3 items in cart --> gets this
            {
                //var remainingQuantity = promotion.MinNumberOfItems - rewardDescriptions.Sum(x => x.or.Quantity);
                var remainingQuantity = promotion.MinNumberOfItems.RequiredQuantity - lineItems.Count();

                return String.Format(CultureInfo.InvariantCulture, "Buy {0} more items and get a {1}% discount!"
                    , remainingQuantity, promotion.PercentageDiscount);
            }

            return "The promotion is not applicable for the current order.";
        }


        #region Older style - not used

        /* New style in latest NuGet*/
        //// affectedItems is a collection - used for getting the names
        //// Returns the names of all affected items combined into a string
        //private string GetContentNames(IEnumerable<AffectedItem> affectedItems)
        //{
        //    var affectedContentLinks = affectedItems.Select(x => x.ContentLink);
        //    var contentNames = _contentLoader.GetItems(affectedContentLinks, CultureInfo.InvariantCulture).Select(x => x.Name);

        //    return String.Join(", ", contentNames);
        //}

        //Injected<ReferenceConverter> _refConv;
        //private object GetContentNames(IEnumerable<AffectedItem> affectedItems)
        /*    private object GetContentNames(IEnumerable<> affectedItems)
        {
            List<ContentReference> affectedContentReferences = new List<ContentReference>();
            var affectedContentCodes = affectedItems.Select(x => x.LineItem.Code);
            foreach (var item in affectedContentCodes)
            {
                affectedContentReferences.Add(_refConv.Service.GetContentLink(item));
            }
            var contentNames = _contentLoader.GetItems(affectedContentReferences, CultureInfo.InvariantCulture).Select(x => x.Name);

            return String.Join(", ", contentNames);
        }*/

        //public virtual RedemptionDescription CreateRedemptionDescriptionText(IOrderForm orderForm)
        //{
        //    // not used right now
        //    // Should check PromotionData.RedemptionLimits Qty and "AffectedEntries"
        //    throw new NotImplementedException();
        //}

        //protected override bool CanBeFulfilled(MyPercentagePromotion promotionData, PromotionProcessorContext context)
        //{
        //    // Should here check if any LineItems are affected in the OrderGroup
        //    return true;
        //}

        //protected override PromotionItems GetPromotionItems(MyPercentagePromotion promotionData)
        //{
        //    // this example is very narrow... but it's for this specific Model/Processor
        //    return new PromotionItems(
        //        promotionData
        //        , new CatalogItemSelection(promotionData.DiscountTargets.Items, CatalogItemSelectionType.All, false)
        //        , new CatalogItemSelection(promotionData.DiscountTargets.Items, CatalogItemSelectionType.All, false)
        //        );
        //}

        #endregion

    }
}
using CommerceTraining.Models.Promotions;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
// added
using EPiServer.Commerce.Marketing.Extensions;
using EPiServer.Commerce.Order;

namespace CommerceTraining.Infrastructure.Promotions
{
    public class FreeItemPromotionProcessor : EntryPromotionProcessorBase<FreeItemPromotion>
    {
        // CollectionTargetEvaluator is used to evaluate an order against a promotion's target properties
        private readonly CollectionTargetEvaluator _targetEvaluator;
        private readonly IContentLoader _contentLoader;
        private readonly FulfillmentEvaluator _fulfillmentEvaluator;
        private readonly LocalizationService _localizationService;
        public FreeItemPromotionProcessor(
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

        Injected<ReferenceConverter> _refConv; // lazy dev - fix
        protected override RewardDescription Evaluate(
             FreeItemPromotion data, PromotionProcessorContext ctx)
        {
            // is this used
            IEnumerable<ContentReference> targetItems =
                data.DiscountTargets.Items.ToList(); // get which LI this promo is for

            // data - the promotion itself, and the custom model provided
            // ctx - gives OrderGroup, OrderForm, Prices (PriceMatrix) & some calc

            var targets = data.DiscountTargets;
            var freeItem = data.FreeItem.Items.First().ToReferenceWithoutVersion(); // just to show some
            var freeItemCode = _refConv.Service.GetCode(freeItem); // Have RefConv. injected below

            IOrderForm form = ctx.OrderForm;
            var lineItems = form.GetAllLineItems();
            //var lineItems = GetLineItems(ctx.OrderForm.get); // not anymore
            var matchRecursive = false; // mandatory

            IList<string> skuCodes = _targetEvaluator.GetApplicableCodes(
                lineItems, targets.Items, targets.MatchRecursive); // get one if kicked in, 0 if not

            FulfillmentStatus status = data.RequiredQty.GetFulfillmentStatus(
                form, _targetEvaluator, _fulfillmentEvaluator); // extension method

            List<RewardDescription> rewardDescriptions = new List<RewardDescription>();
            List<RedemptionDescription> redemptionDescriptions = new List<RedemptionDescription>();

            // GetAllAffectedItems is the only method - name was changed
            var affectedItems = _targetEvaluator.GetApplicableCodes(lineItems, targetItems, matchRecursive);

            // This way (demo) of using it is maybe not the intended, thought of like 
            // "buy 5 - get one for free"
            // ...have to load the gift and add it to the cart
            // ...have "Money", "Percentage", GiftItem, FreeItem etc.
            return RewardDescription.CreateFreeItemReward(
                status
                , GetRedemptions(skuCodes, data, ctx) // explanation below
                , data
                , data.Description + " : " + freeItemCode
                );
        }

        // new style
        private IEnumerable<RedemptionDescription> GetRedemptions(
            IList<string> skuCodes
            , FreeItemPromotion promotionData
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
            var requiredQuantity = promotionData.RequiredQty;
            var maxRedemptions = GetMaxRedemptions(promotionData.RedemptionLimits);

            // have this one above also
            for (int i = 0; i < maxRedemptions; i++)
            {
                // ExtractEntries ... 3:rd argument is "sort", if not defined we get "most expensive" first
                // The ordering might be important, for example in the "Buy 3, get the cheapest for free".
                // ...the method sits on the PriceMatrix
                var affectedEntries = context.EntryPrices.ExtractEntries(
                    skuCodes
                    , 1
                    ,promotionData);

                if (affectedEntries == null)
                {
                    break;
                }
                redemptions.Add(CreateRedemptionDescription(affectedEntries));
            }
            return redemptions;
        }

        // could have some custom stuff here
        protected override RedemptionDescription CreateRedemptionDescription(AffectedEntries affectedEntries)
        {
            return base.CreateRedemptionDescription(affectedEntries);
        }

        // may want to customize
        protected override RewardDescription NotFulfilledRewardDescription(FreeItemPromotion promotionData, PromotionProcessorContext context, FulfillmentStatus fulfillmentStatus)
        {
            return base.NotFulfilledRewardDescription(promotionData, context, fulfillmentStatus);
        }

        // just checking
        protected override PromotionItems GetPromotionItems(FreeItemPromotion promotionData)
        {
            var targets = promotionData.DiscountTargets;
            var coupon = promotionData.Coupon.Code;
            var discountType = promotionData.DiscountType;
            var minNoOfItems = promotionData.RequiredQty;
            var partialFullFilment = promotionData.PartialFulfillmentNumberOfItems;
            var redemptionLimits = promotionData.RedemptionLimits;
            var toString = promotionData.ToString(); // ...the Castle-proxy
            return null;
        }

        protected override bool CanBeFulfilled(FreeItemPromotion promotionData, PromotionProcessorContext context)
        {
            return true;
        }
    }
}
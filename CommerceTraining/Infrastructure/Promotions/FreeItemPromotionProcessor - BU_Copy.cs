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
    // testing GiftItems promo
    //[ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
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
            IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
            _targetEvaluator = targetEvaluator;
            _fulfillmentEvaluator = fulfillmentEvaluator;
            _localizationService = localizationService;
        }

        protected override RewardDescription Evaluate(
             FreeItemPromotion data, PromotionProcessorContext ctx)
        {
            // is this used
            IEnumerable<ContentReference> targetItems =
                data.DiscountTargets.Items.ToList(); // get which LI this promo is for

            var targets = data.DiscountTargets;
            var freeItem = data.FreeItem.Items.First().ToReferenceWithoutVersion();
            //var freeItemCode = ServiceLocator.Current.GetInstance<ReferenceConverter>().GetCode(freeItem);
            var freeItemCode = _refConv.Service.GetCode(freeItem); // Have RefConv. below

            IOrderForm form = ctx.OrderForm;
            var lineItems = form.GetAllLineItems();
            //var lineItems = GetLineItems(ctx.OrderForm.get); // not anymore
            var matchRecursive = false; // mandatory

            var skuCodes = _targetEvaluator.GetApplicableCodes(
                lineItems, targets.Items, targets.MatchRecursive); // get one if kicked in, 0 if not


            FulfillmentStatus status = data.RequiredQty.GetFulfillmentStatus(form, _targetEvaluator, _fulfillmentEvaluator);

            List<RewardDescription> rewardDescriptions = new List<RewardDescription>();
            List<RedemptionDescription> redemptionDescriptions = new List<RedemptionDescription>();

            // GetAllAffectedItems is the only method - name was changed
            var affectedItems = _targetEvaluator.GetApplicableCodes(lineItems, targetItems, matchRecursive);

            // only a single method to use on the "FulfillmentEvaluator"  - out commented
            //var status = FulfillmentEvaluator.GetStatusForBuyQuantityPromotion(affectedItems.Select(x => x.LineItem)
            //    , data.RequiredQty,0); // "Required" in the model 

            // This way (demo) of using it is maybe not the intended, thought of like "buy 5 - get one for free"
            // ...have to load the gift and add it to the cart
            // ...have "Money" and "Percentage" 
            return RewardDescription.CreateFreeItemReward(
                status
                , GetRedemptions(skuCodes, data, ctx)
                , data
                , data.Description + " : " + freeItemCode

                //status,
                //affectedItems,
                //data
                //GetRewardDescriptionText(tagets, status, data)
                );
        }

        //Injected<RedemptionLimitService> r_srv;
        //private IEnumerable<RedemptionDescription> GetRedemptions(IList<string> skuCodes, FreeItemPromotion data, PromotionProcessorContext ctx)
        //{
        //    //throw new NotImplementedException();
        //    //r_srv.Service.GetRemainingRedemptions()
        //    var redemptions = new List<RedemptionDescription>();
        //    var affectedEntries = ctx.EntryPrices.ExtractEntries(
        //        skuCodes
        //        , 1);
        //    redemptions.Add(CreateRedemptionDescription(affectedEntries));
        //    return redemptions;
        //}

        // new
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

        // out commented
        //private string GetRewardDescriptionText(IList<AffectedItem> affectedItems
        //    , Mediachase.Commerce.Marketing.FulfillmentStatus status
        //    , FreeItemPromotion data)
        //{
        //    var contentNames = GetContentNames(affectedItems); // local method

        //    // Check out the free gift
        //    var freeItemName = _contentLoader.Get<EntryContentBase>(data.FreeItem.Items.First()).Name; // only one item here

        //    // only checking "FulFilled" now (have "Partial" in the percentage-promo)
        //    if (status == FulfillmentStatus.Fulfilled)
        //    {
        //        return String.Format(CultureInfo.InvariantCulture, "{0} for free when buying the item '{1}'!"
        //            , freeItemName, contentNames);
        //    }
        //    else
        //    {
        //        return "No give-aways here";
        //    }

        //}

        Injected<ReferenceConverter> _refConv;
        // out commented
        //private object GetContentNames(IList<AffectedItem> affectedItems)
        //{
        //    List<ContentReference> affectedContentReferences = new List<ContentReference>();
        //    var affectedContentCodes = affectedItems.Select(x => x.LineItem.Code);
        //    foreach (var item in affectedContentCodes)
        //    {
        //        affectedContentReferences.Add(_refConv.Service.GetContentLink(item));
        //    }
        //    var contentNames = _contentLoader.GetItems(affectedContentReferences, CultureInfo.InvariantCulture).Select(x => x.Name);

        //    return String.Join(", ", contentNames);
        //}

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
            //throw new NotImplementedException();
            return true;
        }
    }
}
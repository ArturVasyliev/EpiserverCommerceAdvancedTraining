using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// added
using EPiServer.Commerce.Marketing.Extensions;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Marketing;
using CommerceTraining.Models.Promotions;
using EPiServer.Framework.Localization;
using EPiServer;
using Mediachase.Commerce.Catalog;
using EPiServer.Core;

namespace CommerceTraining.Infrastructure.Promotions
{
    public class Free2Proc : EntryPromotionProcessorBase<Free2>
    {
        private readonly CollectionTargetEvaluator _collectionTargetEvaluator;
        private readonly FulfillmentEvaluator _fulfillmentEvaluator;
        private readonly LocalizationService _localizationService;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;

        public Free2Proc(
            CollectionTargetEvaluator collectionTargetEvaluator,
            FulfillmentEvaluator fulfillmentEvaluator,
            LocalizationService localizationService,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            RedemptionDescriptionFactory redemptionDescriptionFactory)
            :base(redemptionDescriptionFactory)
        {
            _contentLoader = contentLoader;
            _collectionTargetEvaluator = collectionTargetEvaluator;
            _fulfillmentEvaluator = fulfillmentEvaluator;
            _localizationService = localizationService;
            _referenceConverter = referenceConverter;
        }

        protected override RewardDescription Evaluate(
            Free2 promotionData
            , PromotionProcessorContext context)
        {

            // RequiredQty & FreeItem in the model

            IOrderForm orderForm = context.OrderGroup.GetFirstForm();
            IEnumerable<ILineItem> lineItems = context.OrderGroup.GetFirstForm().GetAllLineItems();
            ContentReference freeItem = promotionData.FreeItem.Items.First(); // Using "First()" just to show
            string freeItemCode = _referenceConverter.GetCode(freeItem);

            FulfillmentStatus status = promotionData.RequiredQty.GetFulfillmentStatus(
                orderForm, _collectionTargetEvaluator, _fulfillmentEvaluator);

            IList<string> applicableEntryCodes = _collectionTargetEvaluator.GetApplicableCodes(
                lineItems, promotionData.RequiredQty.Items, false); 
            
            string description = promotionData.Description;

            return RewardDescription.CreateFreeItemReward(
                status
                , GetRedemptionDescriptions(promotionData, context, applicableEntryCodes)
                , promotionData
                , description + " : " + freeItemCode); // description, just to show

            
        }

        private IEnumerable<RedemptionDescription> GetRedemptionDescriptions(
            PromotionData promotionData
            ,PromotionProcessorContext context
            , IList<string> applicableEntryCodes)
        {
            List<RedemptionDescription> redemptionDescriptions = 
                new List<RedemptionDescription>();

            var maxRedemptions = GetMaxRedemptions(promotionData.RedemptionLimits);
            var affectedEntries = context.EntryPrices.ExtractEntries(applicableEntryCodes, 1);

            for (int i = 0; i < maxRedemptions; i++)
            {
                if (affectedEntries == null)
                {
                    break;
                }

                redemptionDescriptions.Add(CreateRedemptionDescription(affectedEntries));

            }

            return redemptionDescriptions;
        }

        protected override PromotionItems GetPromotionItems(Free2 promotionData)
        {

            return null;
        }

        //protected override RewardDescription Evaluate(FreeItemPromotion promotionData
        //    , PromotionProcessorContext context)
        //{
        //    // form needed
        //    IOrderForm form = context.OrderForm;

        //    // status needed 
        //    FulfillmentStatus status = promotionData.RequiredQty.GetFulfillmentStatus(
        //        form
        //        , _collectionTargetEvaluator
        //        , _fulfillmentEvaluator
        //        );


        //    return RewardDescription.CreateFreeItemReward(
        //        status
        //        , GetRedemptions()
        //        , promotionData
        //        , "This is the Free2 promotion that kicked in");
        //}


    }
}
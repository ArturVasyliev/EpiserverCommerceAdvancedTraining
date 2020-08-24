using EPiServer;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Extensions;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.Promotions
{
    public class FreeStuffPromotionProcessor : EntryPromotionProcessorBase<FreeStuffPromotion>
    {
        private CollectionTargetEvaluator _collectionTargetEvaluator;
        private FulfillmentEvaluator _fulfillmentEvaluator;
        private GiftItemFactory _giftItemFactory;
        private LocalizationService _localizationService;
        private IContentLoader _contentLoader;
        private ReferenceConverter _referenceConverter;

        public FreeStuffPromotionProcessor(RedemptionDescriptionFactory redemptionDescriptionFactory,
            CollectionTargetEvaluator collectionTargetEvaluator,
            FulfillmentEvaluator fulfillmentEvaluator,
            LocalizationService localizationService,
            GiftItemFactory giftItemFactory,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter
            ) : base(redemptionDescriptionFactory)
        {
            _collectionTargetEvaluator = collectionTargetEvaluator;
            _fulfillmentEvaluator = fulfillmentEvaluator;
            _giftItemFactory = giftItemFactory;
            _localizationService = localizationService;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
        }
        protected override RewardDescription Evaluate(FreeStuffPromotion promotionData, PromotionProcessorContext context)
        {
            var condition = promotionData.RequiredQty;

            var lineItems = context.OrderForm.GetAllLineItems();

            IList<string> skuCodes = _collectionTargetEvaluator.GetApplicableCodes(lineItems,
                condition.Items, false);

            FulfillmentStatus status = promotionData.RequiredQty
                .GetFulfillmentStatus(context.OrderForm, _collectionTargetEvaluator, _fulfillmentEvaluator);

            List<RedemptionDescription> redemptions = new List<RedemptionDescription>();

            if(status == FulfillmentStatus.Fulfilled)
            {
                AffectedEntries entries = _giftItemFactory.CreateGiftItems(promotionData.FreeItem, context);
                redemptions.Add(CreateRedemptionDescription(entries));
            }

            return RewardDescription.CreateGiftItemsReward(status, redemptions,
                promotionData, CreateCustomRewardDescriptionText(status, promotionData));
        }

        protected virtual string CreateCustomRewardDescriptionText(FulfillmentStatus fulfillmentStatus,
            FreeStuffPromotion promotionData)
        {
            string freeItemCode = _referenceConverter.GetCode(promotionData.FreeItem[0]);

            if(fulfillmentStatus == FulfillmentStatus.PartiallyFulfilled)
            {
                List<string> codes = new List<string>();
                int numItems = promotionData.RequiredQty.Items.Count;
                foreach (var item in promotionData.RequiredQty.Items)
                {
                    codes.Add(_referenceConverter.GetCode(item));
                    
                }
                return $"Buy at least {promotionData.RequiredQty.RequiredQuantity} " +
                    $"of {string.Join(",", codes)} and get a free {freeItemCode}!";
            }
            if(fulfillmentStatus == FulfillmentStatus.Fulfilled)
            {
                return $"Congratulations! You get a free {freeItemCode}";
            }
            return "This promotion is not applicable for the current order.";
        }
        protected override bool CanBeFulfilled(FreeStuffPromotion promotionData, PromotionProcessorContext context)
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override PromotionItems GetPromotionItems(FreeStuffPromotion promotionData)
        {
            throw new NotImplementedException();
        }
    }
}
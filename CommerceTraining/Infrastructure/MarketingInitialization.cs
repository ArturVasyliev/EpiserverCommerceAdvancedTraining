using System;
using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Commerce.Marketing;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(CommerceTraining.Infrastructure.EPiServerCommerceInitializationModule))]
    public class MarketingInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            try
            {
                UpdateThresholds();
            }
            catch (Exception)
            {

            }

            var events = ServiceLocator.Current.GetInstance<IContentEvents>();
            events.SavingContent += Events_SavingContent;
        }

        private void Events_SavingContent(object sender, ContentEventArgs e)
        {
            if (e.Content is SpendAmountGetOrderDiscount)
            {
                ((SpendAmountGetOrderDiscount)e.Content).Condition.PartiallyFulfilledThreshold = 0.75m;
                return;
            }

            if (e.Content is SpendAmountGetGiftItems)
            {
                ((SpendAmountGetGiftItems)e.Content).Condition.PartiallyFulfilledThreshold = 0.75m;
                return;
            }

            if (e.Content is SpendAmountGetGiftItems)
            {
                ((SpendAmountGetGiftItems)e.Content).Condition.PartiallyFulfilledThreshold = 0.75m;
                return;
            }

            if (e.Content is SpendAmountGetItemDiscount)
            {
                ((SpendAmountGetItemDiscount)e.Content).Condition.PartiallyFulfilledThreshold = 0.75m;
                return;
            }

            if (e.Content is SpendAmountGetShippingDiscount)
            {
                ((SpendAmountGetShippingDiscount)e.Content).Condition.PartiallyFulfilledThreshold = 0.75m;
                return;
            }
        }

        private static void UpdateThresholds()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var campaigns = contentLoader.GetChildren<SalesCampaign>(SalesCampaignFolder.CampaignRoot);
            foreach (var salesCampaign in campaigns)
            {
                var orderPromotions = contentLoader.GetChildren<OrderPromotion>(salesCampaign.ContentLink);
                foreach (var orderPromotion in orderPromotions)
                {
                    if (orderPromotion is SpendAmountGetOrderDiscount)
                    {
                        var clone = orderPromotion.CreateWritableClone() as SpendAmountGetOrderDiscount;
                        clone.Condition.PartiallyFulfilledThreshold = 0.75m;
                        contentRepository.Save(clone, SaveAction.Publish, AccessLevel.NoAccess);
                    }

                }

                var entryPromotions = contentLoader.GetChildren<EntryPromotion>(salesCampaign.ContentLink);
                foreach (var entryPromotion in entryPromotions)
                {
                    if (entryPromotion is SpendAmountGetGiftItems)
                    {
                        var clone = entryPromotion.CreateWritableClone() as SpendAmountGetGiftItems;
                        clone.Condition.PartiallyFulfilledThreshold = 0.75m;
                        contentRepository.Save(clone, SaveAction.Publish, AccessLevel.NoAccess);
                    }

                    if (entryPromotion is SpendAmountGetItemDiscount)
                    {
                        var clone = entryPromotion.CreateWritableClone() as SpendAmountGetItemDiscount;
                        clone.Condition.PartiallyFulfilledThreshold = 0.75m;
                        contentRepository.Save(clone, SaveAction.Publish, AccessLevel.NoAccess);
                    }
                }
            }
        }
        public void Uninitialize(InitializationEngine context)
        {
            var events = ServiceLocator.Current.GetInstance<IContentEvents>();
            events.SavingContent -= Events_SavingContent;
        }



    }
}
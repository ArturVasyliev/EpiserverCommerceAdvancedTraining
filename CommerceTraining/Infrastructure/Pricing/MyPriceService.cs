using EPiServer.Events.ChangeNotification;
using EPiServer.Framework.Cache;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Engine.Events;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Pricing.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.Pricing
{
    public class MyPriceService : PriceServiceDatabase
    {
        //OLD, not done will go away

        public MyPriceService(IChangeNotificationManager changeManager
            , ISynchronizedObjectInstanceCache objectInstanceCache
            , CatalogKeyEventBroadcaster broadcaster, EntryIdentityResolver entryIdentityResolver, IApplicationContext appContext)
            : base(changeManager, objectInstanceCache, broadcaster, appContext ,entryIdentityResolver)
        { 
            
        
        }



        
        
    }
}
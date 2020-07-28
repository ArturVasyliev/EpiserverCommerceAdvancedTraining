using EPiServer.Commerce.Order;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Engine.Events;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer;
using EPiServer.Events;

namespace CommerceTraining.Infrastructure
{
    // ... a silly little example
    public static class EventReciever
    {
        // need to do more here, like take care of what keys was updated 
        public static void RecordPriceEvent(object sender, PriceUpdateEventArgs e)
        {
            var s = sender.GetType();
            var e1 = e.CatalogKeys.First().ToString();
            var e2 = e.CatalogKeys.First().CatalogEntryCode;
            BfEventManager.SendToBF("Pricing", e1 + ":" + e2, s.ToString());
        }

        public static void RecordInventoryEvent(object sender, InventoryUpdateEventArgs e)
        {
            //var i = sender.GetType().Name;
            //var e1 = e.ApplicationHasContentModelTypes.ToString();
            //var e2 = e.CatalogKeys.First().CatalogEntryCode;
            //BfEventManager.SendToBF("Inventory", e1 + ":" + e2, i.ToString()); // ...i was too long (max 100)

            if (e.CatalogKeys.Count() > 0)
            {
                var i = sender.GetType().Name;
                var e1 = e.ApplicationHasContentModelTypes.ToString();
                var e2 = e.CatalogKeys.First().CatalogEntryCode;
                BfEventManager.SendToBF("Inventory", e1 + ":" + e2, i.ToString()); // ...i was too long (max 100)
            }

        }

        // ...this one is old, have IOrderRepositoryCallback 
        public static void RecordOrderGroupEvent(object sender, OrderGroupEventArgs e)
        {
            var o = sender.GetType();
            var e1 = e.OrderGroupId;
            var e2 = e.OrderGroupType;
            BfEventManager.SendToBF("OrderGroup", e1.ToString(), o.ToString());
        }


        public static void RecordNewOrderEvents(OrderReference orderReference)
        {
            BfEventManager.SendToBF("FrontEnd", orderReference.OrderGroupId.ToString() + " was changed", "NewOrderEvents");
        }

        internal static void RecordCatalogItemApprovalRequest(object sender, ContentEventArgs e)
        {
            BfEventManager.SendToBF(sender.GetOriginalType().Name, e.TargetLink.ToString(), e.Content.ParentLink.ID.ToString());
        }

        public static void RecordEntryChange(object sender, EventNotificationEventArgs e)
        {
            BfEventManager.SendToBF(
                sender.GetOriginalType().Name
                , "Entry changed"
                ,e.ToString());
        }

        
    }

    public static class BfEventManager
    {

        //public static
        // create a EventLog in BF
        public static void SendToBF(string origin, string message, string source)
        {
            EntityObject e = BusinessManager.InitializeEntity("EventLog");
            e["Title"] = origin;
            e["Message"] = message;
            e["Source"] = source;
            BusinessManager.Create(e);
        }



    }
}
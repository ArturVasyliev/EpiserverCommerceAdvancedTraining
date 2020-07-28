using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.CartAndCheckout
{
    [ServiceConfiguration(typeof(IOrderRepositoryCallback), 
        Lifecycle = ServiceInstanceScope.Singleton)]
    public class NewOrderEvents : IOrderRepositoryCallback
    {
        public void OnCreated(OrderReference orderReference)
        {
            //throw new NotImplementedException();
        }

        public void OnCreating(Guid customerId, string name)
        {
            //throw new NotImplementedException();
        }

        public void OnDeleted(OrderReference orderReference)
        {
            //throw new NotImplementedException();
            
        }

        public void OnDeleting(OrderReference orderReference)
        {
            //throw new NotImplementedException();
        }

        public void OnUpdated(OrderReference orderReference)
        {
            //throw new NotImplementedException();
            EventReciever.RecordNewOrderEvents(orderReference);
        }

        public void OnUpdating(OrderReference orderReference)
        {
            //throw new NotImplementedException();
        }
    }
}
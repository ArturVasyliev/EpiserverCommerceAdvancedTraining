using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure
{
    /* Not done yet... continue when time permits, just a quick fix here */
    public static class MyCustomMarketService
    {
        public static CustomerContact GetOwner(string marketId, out bool foundOne)
        {
            //ToDo: create the field in BF for the contact
            // ...may not need the "out-part"
            FilterElementCollection fc = new FilterElementCollection
            {
                FilterElement.EqualElement("MarketToOwn", marketId)
            };

            EntityObject[] result = BusinessManager.List(CustomerContact.ClassName, fc.ToArray(), null);

            if (result != null)
            {
                if (result.Count() == 1)
                {
                    // got the contact
                    foundOne = true;
                    return (CustomerContact)result.FirstOrDefault();
                }
                else { } // could be several owners, need some action
            }
            else { } // empty

            foundOne = false;
            return null;
        }
    }
}
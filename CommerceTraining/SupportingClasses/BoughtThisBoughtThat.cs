using EPiServer.Find;
using SpecialFindClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public class BoughtThisBoughtThat
    {
        
        public IEnumerable<string> GetItems(string entryCode)
        {
            List<string> localList = new List<string>();

            IClient client = Client.CreateFromConfig(); 

            var result = client.Search<OrderValues>() 
                .For(entryCode)
                .InField("LineItemCodes")
                .GetResult();

            // can do smarter, but it´s explicit :)
            foreach (var item in result)
            {
                foreach (var item2 in item.LineItemCodes)
                {
                    if (item2 != entryCode) // excluding what was searched for
                    {
                        if (localList.Contains(item2))
                        {
                            // do nothing
                        }
                        else // add it
                        {
                            localList.Add(item2);
                        }

                    }
                }
            }

            return localList;
        }

    }
}
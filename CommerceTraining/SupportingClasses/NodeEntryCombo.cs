using EPiServer.Commerce.Catalog.ContentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public class NodeEntryCombo
    {
        public IEnumerable<NameAndUrls> nodes { get; set; }

        public IEnumerable<NameAndUrls> entries { get; set; }

        public string promotionMessage { get; set; }
    }
}
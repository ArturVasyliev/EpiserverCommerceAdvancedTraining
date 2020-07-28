using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public class NameAndUrls
    {
        public string name { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
        public string imageTumbUrl { get; set; }
    }
}
using EPiServer.Commerce.Catalog.ContentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class NodeListingBlockViewModel
    {
        public string Name { get; set; }
        public IEnumerable<EntryContentBase> entries { get; set; }
    }
}
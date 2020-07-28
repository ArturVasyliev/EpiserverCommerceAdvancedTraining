using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class RelatedStuffBlockViewModel
    {
        public string Name { get; set; }
        public string RelatingTo { get; set; }
        public string AssociationGroup { get; set; } // for a heading - showing the filter
        public string Group { get; set; } // for a picker of Group
        public string Type { get; set; } // for another picker and further options
        public string theContent { get; set; }
        public string theParentContent { get; set; }
        public IEnumerable<EPiServer.Core.ContentReference> associations { get; set; }
        public EPiServer.Core.ContentReference finalRef { get; set; }
        public EPiServer.Commerce.Catalog.ContentTypes.EntryContentBase theEntryContentBase { get; set; }
    }
}
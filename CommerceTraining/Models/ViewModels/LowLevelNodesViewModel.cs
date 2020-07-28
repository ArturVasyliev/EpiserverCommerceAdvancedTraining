using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class LowLevelNodesViewModel
    {
        public XhtmlString modelMainBody { get; set; }
        public  IEnumerable<ContentReference> nodeReferences { get; set; }

        public ContentArea productArea { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public class AssociationModel
    {
        public string text { get; set; }
        public EPiServer.Core.ContentReference theRef { get; set; }
        public string url { get; set; }
    }
}
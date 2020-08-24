using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class ProjectedShirtViewModel
    {
        public string SearchText { get; set; }
        public string ResultCount { get; set; }
        public List<ProjectedShirt> Shirts { get; set; }
    }

    public class ProjectedShirt
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public ContentReference UrlLink { get; set; }
    }
}
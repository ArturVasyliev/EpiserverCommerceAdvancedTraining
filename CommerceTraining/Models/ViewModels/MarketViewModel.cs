using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;
using Mediachase.Commerce;

namespace CommerceTraining.Models.ViewModels
{
    public class MarketViewModel
    {

        public IEnumerable<SelectListItem> Markets { get; set; }
        public string MarketId { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}
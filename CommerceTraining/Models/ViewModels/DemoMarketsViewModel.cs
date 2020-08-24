using CommerceTraining.Models.Catalog;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class DemoMarketsViewModel
    {
        public IMarket SelectedMarket { get; set; }
        public IEnumerable<IMarket> MarketList { get; set; }
        public ShirtVariation Shirt { get; set; }

    }
}
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class CartViewModel
    {
        public IEnumerable<ILineItem> LineItems { get; set; }
        public Money CartTotal { get; set; }
        public string Messages { get; set; }
        public string PromotionMessages { get; set; }
        public string AgreeWithSplitPayment { get; set; } // RoCe: should be bool
    }
}
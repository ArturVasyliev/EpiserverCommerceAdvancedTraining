using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommerceTraining.Models.Pages
{
    public class CheckOutViewModel
    {
        
        public CheckOutPage CurrentPage { get; set; }

        // Lab E1 - create properties below
        public IEnumerable<PaymentMethodDto.PaymentMethodRow> PaymentMethods { get; set; }
        public IEnumerable<ShippingMethodDto.ShippingMethodRow> ShipmentMethods { get; set; }
        public IEnumerable<ShippingRate> ShippingRates { get; set; }

        public Guid SelectedShipId { get; set; }
        public Guid SelectedPayId { get; set; }

        // new for Adv.
        public string ShippingMethodInfo { get; set; }
        
        public CheckOutViewModel()
        { }

        public CheckOutViewModel(CheckOutPage currentPage)
        {
            CurrentPage = currentPage;
        }


    }
}
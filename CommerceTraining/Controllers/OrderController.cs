using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;

namespace CommerceTraining.Controllers
{
    public class OrderController : PageController<OrderPage>
    {
        public ActionResult Index(OrderPage currentPage, string passedAlong)
        {
            var model = new OrderViewModel()
            {
                TrackingNumber = passedAlong
            };

            return View(model);
        }
    }
}
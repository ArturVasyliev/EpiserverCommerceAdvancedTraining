using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Pricing;
using EPiServer.Commerce.Marketing;
using Mediachase.Commerce;
using CommerceTraining.Models.Catalog;
using Mediachase.Commerce.Catalog;
using EPiServer.Security;
using Mediachase.Commerce.Security;
//using EPiServer.Security;
using Mediachase.Commerce.Orders.Managers;
using System;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Customers;
using System.Net.NetworkInformation;
using CommerceTraining.SupportingClasses;
using Mediachase.Commerce.Markets;
using CommerceTraining.Models.ViewModels;
using Owin;
using EPiServer.Filters;
using EPiServer.Commerce.Catalog.Linking;
using Mediachase.Commerce.InventoryService;
using System.Security.Principal;

namespace CommerceTraining.Controllers
{
    public class StartPageController : PageController<StartPage>
    {
        public readonly IContentLoader _contentLoader;
        public readonly UrlResolver _urlResolver;
        public readonly ICurrentMarket _currentMarketService;
        public readonly IMarketService _marketService;

        ContentReference TopCategory { get; set; } // used for listing of nodes at the start-page

        public StartPageController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , ICurrentMarket currentMarket
            , IMarketService marketService)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _currentMarketService = currentMarket;
            _marketService = marketService;

            // uncomment the below when the catalog is modelled
            TopCategory = contentLoader.Get<StartPage>(PageReference.StartPage).Settings.topCategory;
        }

        // A check for FInd
        private bool IsOnLine()
        {
            //return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            return false;
        }

        public string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

        public ActionResult Index(StartPage currentPage, string selectedMarket)
        {
            TempData["IsOnLine"] = CheckIfOnLine.IsInternetAvailable; // not the best way... improve this

            if (selectedMarket != null)
            {
                _currentMarketService.SetCurrentMarket(new MarketId(selectedMarket)); // outcommented in Adv.-starter
            }

            
            var model = new CommerceTraining.Models.ViewModels.PageViewModel<StartPage>(currentPage)
            {
                MainBodyStartPage = currentPage.MainBody,
                myPageChildren = _contentLoader.GetChildren<IContent>(currentPage.ContentLink),
                Customer = LoggedInOrNot(),

                // uncomment the below when the catalog is modelled
                topLevelCategories = FilterForVisitor.Filter(
                _contentLoader.GetChildren<CatalogContentBase>(TopCategory).OfType<NodeContent>()),
                markets = _marketService.GetAllMarkets(),
                selectedMarket = _currentMarketService.GetCurrentMarket().MarketName,
                //selectedMarket = MarketId.Default.Value - starter in Adv.
                someInfo = GetStringInfo(currentPage)
            };

            return View(model);
        }

        Injected<IOrderGroupFactory> _factory;
        Injected<IOrderRepository> _repo;
        Injected<IInventoryProcessor> _processor;
        private void CheckInventory2()
        {
            string entryCode = "PriceTest_1";

            ICart cart = _repo.Service.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");
            ILineItem li = _factory.Service.CreateLineItem(entryCode, cart);
            li.Quantity = 1;
            cart.AddLineItem(li);
            cart.GetFirstShipment().WarehouseCode = "test";
            _repo.Service.Save(cart);

            var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

            cart.AdjustInventoryOrRemoveLineItems((item, issue) => validationIssues.Add(item, issue));

            _repo.Service.Save(cart);

            
        }

        Injected<IInventoryService> invSrvs;
        private void CheckInventory()
        {
            string warehouseCode = "Test";
            string entryCode = "PriceTest_1";
            int quantity = 2;

            List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
            InventoryRequestItem requestItem = new InventoryRequestItem
            {
                CatalogEntryCode = entryCode,
                Quantity = quantity,
                WarehouseCode = warehouseCode,
                RequestType = InventoryRequestType.Purchase // reserve for now
            }; // The one we use now
            requestItems.Add(requestItem);

            InventoryRequest inventoryRequest =
                new InventoryRequest(DateTime.UtcNow, requestItems, null);
            InventoryResponse inventoryResponse = invSrvs.Service.Request(inventoryRequest);
            bool theBool = false;

            if (inventoryResponse.IsSuccess)
            {
                theBool = inventoryResponse.IsSuccess;
            }
            else
            {

                InventoryResponseItem iii = inventoryResponse.Items.FirstOrDefault();
                InventoryResponseTypeInfo typeInfo = iii.ResponseTypeInfo;

            }
        }

        private IEnumerable<string> GetStringInfo(StartPage currentPage)
        {
            List<string> localInfo = new List<string>();
            localInfo.Add("Shipping info");

            return localInfo;
        }

        private string LoggedInOrNot()
        {
            if (CustomerContext.Current.CurrentContact != null)
            {
                return "Hi " + CustomerContext.Current.CurrentContact.FirstName;
            }
            else
            {
                return "Hi anonymous, join the club";
            }
        }

    }
}
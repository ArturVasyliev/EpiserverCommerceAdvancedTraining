using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.Security;
using EPiServer.Find;
using System.Collections;
using EPiServer.Commerce.Catalog.ContentTypes;
//using EPiServer.Find.Cms.SearchRequestExtensions;
using EPiServer.Find.Commerce;
using EPiServer.Find.Framework;

namespace CommerceTraining.Controllers
{
    //public class FindUnifiedController : PageController<SearchPage>
    //{
    //    public ActionResult Index(SearchPage currentPage)
    //    {
    //        /* Implementation of action. You can create your own view model class that you pass to the view or
    //         * you can pass the page type for simpler templates */
            
    //        var SearchQuery ="Shirt";

    //        var unifiedSearch = SearchClient.Instance.UnifiedSearchFor(SearchQuery);
    //        var r = unifiedSearch.GetResult();

    //        var model = new FindUnifiedViewModel
    //        {
    //            //unifiedSearch = SearchClient.Instance.UnifiedSearchFor(searchQuery);
    //            //SearchQuery =""
    //            //Results = SearchClient.Instance.UnifiedSearchFor(SearchQuery)
    //            Results = r
    //        };

    //        //if (EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("VisitedPlan"))
    //        //{

    //        //    IClient client = Client.CreateFromConfig();

    //        //    model.MPageDataCollection = new PageDataCollection();

    //        //    var myPageDataCollection = client.Search<IContent>().For("Shirt")
    //        //       .InField(x => x.Name)
    //        //       .Take(50)
    //        //        .GetContentResult()();// .Items.ToList();
    //        //    foreach (VariationContent page in myPageDataCollection)
    //        //    {
    //        //        model.MPageDataCollection.Add(page);
    //        //    }

    //        //}
    //        //else
    //        //{
    //        //    IClient client = Client.CreateFromConfig();

    //        //    model.MPageDataCollection = new PageDataCollection();

    //        //    var myPageDataCollection = client.Search<VariationContent>()
    //        //        .Take(50)
    //        //        .GetResult(); // .Items.ToList();
    //        //    foreach (VariationContent page in myPageDataCollection)
    //        //    {
    //        //        model.MPageDataCollection.Add(page);
    //        //    }

    //        //}

    //        //EPiServer.Find.Cms.SearchRequestExtensions.GetContentResult()

    //        //var visitorGroups = new VisitorGroupStore().List().OrderBy(v => v.Name);
    //        //foreach (var group in visitorGroups)
    //        //{
    //        //    bool isInRole = new VisitorGroupHelper().IsPrincipalInGroup(PrincipalInfo.CurrentPrincipal, group.Name);

    //        //    bool match = EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("VisitedPlan", SecurityEntityType.VisitorGroup);
    //        //}





    //        return View(currentPage);
    //    }




    //}
}
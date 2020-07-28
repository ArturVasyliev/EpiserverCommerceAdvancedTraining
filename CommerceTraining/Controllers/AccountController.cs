using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using EPiServer.Web.Routing;
using CommerceTraining.Models.ViewModels;
using CommerceTraining.SupportingClasses;
using Mediachase.Commerce.Customers;
using System.Web.Security;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Customers.Profile;
using Mediachase.Commerce.Core;
using System.Web;
using System;
using EPiServer.Filters;

namespace CommerceTraining.Controllers
{
    public class AccountController : PageController<AccountPage>
    {
        private readonly IContentRepository _contentRepository;
        private readonly UrlResolver _urlResolver;

        public AccountController(IContentRepository contentRepository, UrlResolver urlResolver)
        {
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
        }


        public ActionResult Index(AccountPage currentPage)
        {
            AccountViewModel model = new AccountViewModel();

            return View(model);
        }

        public ActionResult Login(AccountPage currentPage, string userName, string passWord)
        {
            if (Membership.ValidateUser(userName, passWord))
            {
                MembershipUser account = Membership.GetUser(userName);
                if (userName != null)
                {
                    var profile = SecurityContext.Current.CurrentUserProfile as CustomerProfileWrapper;
                    if (profile != null)
                    {
                        CreateAuthenticationCookie(ControllerContext.HttpContext, userName,"CommerceTraining",true);
                        //return Redirect(url);
                    }
                }
            }

            // ...just for a check
            return RedirectToAction("Index", new { page = ContentReference.StartPage }.page.ToPageReference());
        }

        // ToDo: Lab incustomers module
        public ActionResult CreateAccount(AccountPage currentPage, string userName, string passWord)
        {
            // The important here are the Roles and the Contact properties 
            string firstName = userName;
            string lastName = userName;
            string emailAddress = firstName + "." + lastName + "@epi.com"; // 
            string password = passWord;

            MembershipUser membershipGuy = null;

            MembershipCreateStatus createStatus;
            membershipGuy = Membership.CreateUser(emailAddress, password, emailAddress,
                                         null, null, true, out createStatus);

            // Create the Contact in ECF 
            var customerContact = CustomerContact.CreateInstance(membershipGuy);
            customerContact.FirstName = firstName;
            customerContact.LastName = lastName;
            customerContact.RegistrationSource = String.Format("{0}, {1}"
                , this.Request.Url.Host, SiteContext.Current);

            //customerContact["Email"] = emailAddress; // can do...
            customerContact.Email = emailAddress;

            // Do the "SaveChanges()" before setting ECF-"Roles" 
            customerContact.SaveChanges();

            // These Roles are ECF specific ... saved automatically ... and obsolete in 9
            //SecurityContext.Current.AssignUserToGlobalRole(membershipGuy, AppRoles.EveryoneRole); // For CM
            //SecurityContext.Current.AssignUserToGlobalRole(membershipGuy, AppRoles.RegisteredRole); // For CM

            // do we need anything more?
            Roles.AddUserToRole(membershipGuy.UserName, AppRoles.EveryoneRole);
            Roles.AddUserToRole(membershipGuy.UserName, AppRoles.RegisteredRole);

            // Call for further properties to be set
            SetContactProperties(customerContact);

            return null; // for now
        }

        protected void SetContactProperties(CustomerContact contact)
        {
            Organization org = Organization.CreateInstance();
            org.Name = "ParentOrg";
            org.SaveChanges();

            // Remember the EffectiveCustomerGroup!
            contact.CustomerGroup = "MyBuddies";

            // The custom field
            contact["Geography"] = "West";
            contact.OwnerId = org.PrimaryKeyId;

            contact.SaveChanges();
        }

        public static void CreateAuthenticationCookie(HttpContextBase httpContext, string username, string domain, bool remember)
        {
            // ... needed for cookieless authentication
            FormsAuthentication.SetAuthCookie(username, remember);
            var expirationDate = FormsAuthentication.GetAuthCookie(username, remember).Expires;

            // we need to handle ticket ourselves since we need to save session paremeters as well
            int timeout = httpContext.Session != null ? httpContext.Session.Timeout : 20;
            var ticket = new FormsAuthenticationTicket(2,
                    username,
                    DateTime.Now,
                    expirationDate == DateTime.MinValue ? DateTime.Now.AddMinutes(timeout) : expirationDate,
                    remember,
                    domain,
                    FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // remove the cookie, if one already exists with the same cookie name
            if (httpContext.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                httpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            }

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;

            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Secure = FormsAuthentication.RequireSSL;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            // Create the cookie.
            httpContext.Response.Cookies.Set(cookie);
        }

    }
}
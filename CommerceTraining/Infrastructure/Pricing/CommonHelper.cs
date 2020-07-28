using System.Web;
using System.Web.UI;
using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Search;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.MetaDataPlus.Configurator;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Customers.Profile;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce;

namespace CommerceTraining.Infrastructure.Pricing
{
    // this one is old
    public class CommonHelper
    {

        private const string _CompareCookieItemsName = "MediachaseCompareItems_49CF9917-752F-40a0-AE03-4DEBA4F8F035";
        private const string _CompareCookieMetaClassName = "MediachaseCompareMetaClass_49CF9917-752F-40a0-AE03-4DEBA4F8F035";
        private const int _MaxProductsToCompare = 50;

        #region SafeSelect
        /// <summary>
        /// Safes the select.
        /// </summary>
        /// <param name="ddl">The DDL.</param>
        /// <param name="val">The val.</param>
        public static void SafeSelect(ListControl ddl, string val)
        {
            ListItem li = ddl.Items.FindByValue(val);
            if (li != null)
                li.Selected = true;
        }
        #endregion

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <param name="userGuid">The user GUID.</param>
        /// <returns></returns>
        public static string GetUserName(Guid userGuid)
        {
            string retVal = userGuid.ToString();
            MembershipUser user = Membership.GetUser(userGuid);
            CustomerContact customerContact = null;
            if (user != null)
            {
                retVal = user.UserName;
                customerContact = CustomerContext.Current.GetContactForUser(user);
            }
            if (customerContact != null)
            {
                retVal = customerContact.FullName;
            }
            return retVal;
        }

        /// <summary>
        /// Selects item in the drop down list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="val">The val.</param>
        public static void SelectListItem(DropDownList list, object val)
        {
            if (list.Items.Count == 0)
                return;

            ListItem li = list.SelectedItem;
            if (li != null)
                li.Selected = false;

            // select another item
            if (val != null)
            {
                li = list.Items.FindByValue(val.ToString());
                if (li != null)
                    li.Selected = true;
            }
        }

        /// <summary>
        /// Selects item in the radio list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="val">The val.</param>
        public static void SelectRadioListItem(RadioButtonList list, object val)
        {
            ListItem li = list.SelectedItem;
            if (li != null)
                li.Selected = false;

            // select another item
            if (val != null)
            {
                li = list.Items.FindByValue(val.ToString());
                if (li != null)
                    li.Selected = true;
            }
        }

        /// <summary>
        /// Selects item in the listbox
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="val">The val.</param>
        /// <param name="clearSelection">if set to <c>true</c> [clear selection].</param>
        public static void SelectListItem(ListBox list, object val, bool clearSelection)
        {
            if (list.Items.Count == 0)
                return;

            if (clearSelection)
            {
                foreach (ListItem item in list.Items)
                {
                    if (item.Selected)
                        item.Selected = false;
                }
            }

            // select another item
            if (val != null)
            {
                ListItem li = list.Items.FindByValue(val.ToString());
                if (li != null)
                    li.Selected = true;
            }
        }

        /// <summary>
        /// Adds the linked style sheet.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="styleSheetUrl">The style sheet URL.</param>
        public static void AddLinkedStyleSheet(Page page, string styleSheetUrl)
        {
            AddLinkedStyleSheet(page, styleSheetUrl, true);
        }

        /// <summary>
        /// Adds the linked style sheet.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="styleSheetUrl">The style sheet URL.</param>
        /// <param name="resolveClientUrl">if set to <c>true</c> [resolve client URL].</param>
        public static void AddLinkedStyleSheet(Page page, string styleSheetUrl, bool resolveClientUrl)
        {
            HtmlLink link = new HtmlLink();
            link.Attributes["rel"] = "stylesheet";
            link.Attributes["type"] = "text/css";

            if (resolveClientUrl)
                link.Href = page.ResolveUrl(styleSheetUrl);
            else
                link.Href = styleSheetUrl;

            page.Header.Controls.Add(link);
        }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetIcon(string fileName)
        {
            string extension = String.Empty;
            if (fileName.IndexOf(".") > 0)
                extension = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();
            else
                extension = fileName.ToLower();

            string path = String.Format("~/images/FileType/{0}.gif", extension);

            if (File.Exists(HttpContext.Current.Server.MapPath(path)))
                return path;

            return "~/Images/FileType/default.icon.gif";
        }

        /// <summary>
        /// Gets the flag icon.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string GetFlagIcon(CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(culture.Name) && culture.Name.Length >= 4)
            {
                string extension = culture.Name.Substring(3);

                string path = String.Format("~/_cmsbase/images/Flags/{0}.gif", extension);

                if (File.Exists(HttpContext.Current.Server.MapPath(path)))
                    return path;

                extension = culture.TwoLetterISOLanguageName;

                path = String.Format("~/_cmsbase/images/Flags/{0}.gif", extension);
                if (File.Exists(HttpContext.Current.Server.MapPath(path)))
                    return path;
            }

            return "~/_cmsbase/Images/Flags/-.gif";
        }

        /// <summary>
        /// Returns absolute url by given relative url
        /// </summary>
        /// <param name="relativeUrl">Url, relative to theme folder.</param>
        /// <param name="page">The web page.</param>
        /// <returns></returns>
        public static string GetImageUrl(string relativeUrl, Page page)
        {
            string imageUrl = String.Empty;

            string theme = "Everything";// String.IsNullOrEmpty(page.Theme) ? "Default" : page.Theme; // TODO: store theme somewhere
            string path = page.MapPath(String.Format("~/Templates/{0}/Styles/{1}", theme, relativeUrl));

            if (System.IO.File.Exists(path)) // try current theme
                imageUrl = String.Format("~/Templates/{0}/Styles/{1}", theme, relativeUrl);
            else // try default theme
                imageUrl = String.Format("~/Templates/{0}/Styles/{1}", "Default", relativeUrl);

            return imageUrl;
        }

        /// <summary>
        /// Formats the query string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static string FormatQueryString(string url, NameValueCollection parameters)
        {
            NameValueCollection queryString = new NameValueCollection();

            // Construct the query string from the URL requested
            if (url.Contains("?"))
            {
                string queryParams = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
                string[] paramArr = queryParams.Split(new char[] { '&' });
                foreach (string paramString in paramArr)
                {
                    string[] paramStringArr = paramString.Split(new char[] { '=' });
                    if (paramStringArr.Length > 1)
                        queryString.Add(paramStringArr[0], paramStringArr[1]);
                    else
                        queryString.Add(paramStringArr[0], String.Empty);

                }
            }

            // Replace existing parameters
            if (parameters != null)
            {
                foreach (string key in parameters)
                {
                    queryString.Remove(key);
                    // don't add parameters with empty values to the query string
                    if (!String.IsNullOrEmpty(parameters[key]))
                        queryString.Add(key, parameters[key]);
                }
            }

            // Return the new url
            foreach (string key in queryString)
            {
                if (url.Contains("?"))
                {
                    url += "&" + key + "=" + queryString[key];
                }
                else
                    url += "?" + key + "=" + queryString[key];
            }

            // Return modified URL
            return url;
        }

        #region Create cookies
        /// <summary>
        /// Remembers productId in the cookie
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="mcId"></param>
        /// <returns>Return value >=0 - success, returns amount of products to compare for the specified metaclass; 
        /// -100 - product is already added;
        /// -101 - max number of products reached (=_MaxProductsToCompare)
        /// </returns>
        public static int SetCompareCookie(string productId, string mcId)
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie == null)
                compareCookie = new HttpCookie(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            else
            {
                // check that we don't add the product from different metaclass
                //string[] mcValues = compareCookie.Values.GetValues(mcId);
                //if (mcValues != null && mcValues.Length > 0 && String.Compare(mcValues[0], mcId) != 0)
                //    return -102;
            }

            string[] values = compareCookie.Values.GetValues(mcId);
            bool found = false;

            int returnValue = 0;

            // max products to compare = _MaxProductsToCompare
            if (values != null)
            {
                if (values.Length > 0 && values.Length < _MaxProductsToCompare)
                {
                    // check that the value we're trying to add is not already in the collection
                    foreach (string value in values)
                        if (value == productId)
                        {
                            found = true;
                            returnValue = -100;
                            break;
                        }
                }
                else returnValue = -101;
            }

            if (!found && (values == null || (values != null && values.Length < _MaxProductsToCompare)))
                compareCookie.Values.Add(mcId, productId);

            compareCookie.Expires = DateTime.Now.AddDays(1);

            HttpContext.Current.Response.Cookies.Set(compareCookie);

            if (returnValue != 0)
                return returnValue;
            else
                return compareCookie.Values != null ? compareCookie.Values.GetValues(mcId).Length : 0;
        }

        /// <summary>
        /// Removes product from the cookie.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="mcId">Id of the product's metaclass.</param>
        /// <returns></returns>
        public static int RemoveProductFromCookie(string productId, string mcId)
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie == null)
                return 0;

            // copy all the values to newValues
            // find and remove productId from cookie values
            string[] values = compareCookie.Values.GetValues(mcId);
            List<string> newValues = new List<string>();

            if (values != null && values.Length > 0)
            {
                newValues.AddRange(values);
                if (newValues.Contains(productId))
                    newValues.Remove(productId);
            }

            // set the values without productId
            compareCookie.Values.Remove(mcId);
            foreach (string val in newValues)
                compareCookie.Values.Add(mcId, val);

            compareCookie.Expires = DateTime.Now.AddDays(1);

            HttpContext.Current.Response.Cookies.Set(compareCookie);

            return newValues.Count;
        }

        /// <summary>
        /// Removes all products from the cookie.
        /// </summary>
        public static void ClearCompareCookie()
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null)
            {
                compareCookie.Values.Clear();
                compareCookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Set(compareCookie);
            }
        }

        /// <summary>
        /// Removes all products from the cookie for a specified metaclass.
        /// </summary>
        /// <param name="mcId">Id of the metaclass.</param>
        public static void ClearCompareCookie(string mcId)
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null)
            {
                string[] values = compareCookie.Values.GetValues(mcId);
                if (values != null && values.Length > 0)
                {
                    compareCookie.Values.Remove(mcId);
                    compareCookie.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Set(compareCookie);
                }
            }
        }

        /// <summary>
        /// Returns products to compare.
        /// </summary>
        /// <param name="mcId">Id of a metaClass.</param>
        /// <returns></returns>
        public static CatalogEntryDto GetCompareProductsDto(string mcId)
        {
            CatalogEntryDto products = null;
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null && compareCookie.Values != null)
            {
                List<int> productIds = new List<int>();

                string[] values = compareCookie.Values.GetValues(mcId);
                if (values != null && values.Length > 0)
                {
                    foreach (string productId in values)
                    {
                        int prodId = Int32.Parse(productId);
                        if (!productIds.Contains(prodId))
                            productIds.Add(prodId);
                    }
                }

                // get the products
                CatalogSearchParameters pars = new CatalogSearchParameters();
                CatalogSearchOptions options = new CatalogSearchOptions();

                options.CacheResults = true;
                options.StartingRecord = 0;
                options.RecordsToRetrieve = _MaxProductsToCompare;
                options.ReturnTotalCount = true;
                pars.SqlWhereClause = String.Format("ClassTypeId='{0}'", EntryType.Product);

                // add productids to where statement, since we need to return only selected products
                pars.SqlWhereClause += BuildWhereStatementForSearch(productIds);

                int totalCount = 0;
                products = CatalogContext.Current.FindItemsDto(pars, options, ref totalCount, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull));
            }
            return products;
        }

        /// <summary>
        /// Returns products to compare.
        /// </summary>
        /// <returns></returns>
        public static Entries GetCompareProducts()
        {
            return GetCompareProducts(null);
        }

        /// <summary>
        /// Returns products to compare.
        /// </summary>
        /// <param name="mcId">Id of a metaClass.</param>
        /// <returns></returns>
        public static Entries GetCompareProducts(string mcId)
        {
            Entries products = new Entries();
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null && compareCookie.Values != null)
            {
                List<int> productIds = new List<int>();

                string[] values = new string[] { };
                if (String.IsNullOrEmpty(mcId))
                {
                    List<string> _values = new List<string>();
                    foreach (string key in compareCookie.Values.AllKeys)
                    {
                        if (!String.IsNullOrEmpty(key))
                        {
                            string[] valuesByKey = compareCookie.Values.GetValues(key);
                            _values.AddRange(valuesByKey);

                        }
                    }
                    values = _values.ToArray();
                }
                else
                    values = compareCookie.Values.GetValues(mcId);

                if (values != null && values.Length > 0)
                {
                    foreach (string productId in values)
                    {
                        int prodId = Int32.Parse(productId);
                        if (!productIds.Contains(prodId))
                            productIds.Add(prodId);
                    }
                }

                if (productIds.Count == 0)
                    return products;

                // get the products
                CatalogSearchParameters pars = new CatalogSearchParameters();
                CatalogSearchOptions options = new CatalogSearchOptions();

                options.CacheResults = true;
                options.StartingRecord = 0;
                options.RecordsToRetrieve = _MaxProductsToCompare;
                options.ReturnTotalCount = true;
                //pars.SqlWhereClause = String.Format("ClassTypeId='{0}'", "product");

                pars.SqlWhereClause = String.Format("1=1");

                // add productids to where statement, since we need to return only selected products
                pars.SqlWhereClause += BuildWhereStatementForSearch(productIds);

                products = CatalogContext.Current.FindItems(pars, options, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull));
            }

            return products;
        }

        /// <summary>
        /// Returns products to compare.
        /// </summary>
        /// <returns></returns>
        public static List<EntryGroupCollection> GetCompareGroupedProducts()
        {
            List<EntryGroupCollection> groups = new List<EntryGroupCollection>();
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null && compareCookie.Values != null)
            {
                Entries entries = GetCompareProducts();
                if (entries.Entry != null && entries.Entry.Length > 0)
                {
                    foreach (string key in compareCookie.Values.AllKeys)
                    {
                        if (!String.IsNullOrEmpty(key))
                        {
                            string[] values = compareCookie.Values.GetValues(key);

                            if (values != null && values.Length > 0)
                            {
                                EntryGroupCollection group = new EntryGroupCollection();
                                MetaClass mc = MetaClass.Load(CatalogContext.MetaDataContext, key);
                                if (mc != null)
                                {
                                    group.MetaClassName = key;
                                    group.MetaClassFriendlyName = mc.FriendlyName;
                                    group.Entries = new List<Entry>();

                                    foreach (string productId in values)
                                    {
                                        foreach (Entry entry in entries.Entry)
                                        {
                                            if (entry.CatalogEntryId.ToString().Equals(productId))
                                                group.Entries.Add(entry);
                                        }
                                    }

                                    groups.Add(group);
                                }
                            }
                        }
                    }
                }
            }
            return groups;
        }

        /// <summary>
        /// Builds the where statement for search.
        /// </summary>
        /// <param name="productIds">The product ids.</param>
        /// <returns></returns>
        private static string BuildWhereStatementForSearch(List<int> productIds)
        {
            StringBuilder whereStmtBuilder = new StringBuilder();
            if (productIds.Count == 1)
                whereStmtBuilder.AppendFormat(" AND CatalogEntryId={0}", productIds[0]);
            else if (productIds.Count > 0)
            {
                whereStmtBuilder.Append(" AND(");
                for (int i = 0; i < productIds.Count - 1; i++)
                    whereStmtBuilder.AppendFormat("CatalogEntryId={0} OR ", productIds[i]);

                whereStmtBuilder.AppendFormat("CatalogEntryId={0}", productIds[productIds.Count - 1]);

                whereStmtBuilder.Append(")");
            }
            return whereStmtBuilder.ToString();
        }

        /// <summary>
        /// Returns products to compare.
        /// </summary>
        /// <param name="mcId">Id of a metaClass.</param>
        /// <returns></returns>
        public static List<int> GetCompareProductsIds(string mcId)
        {
            List<int> productIds = new List<int>();
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null && compareCookie.Values != null)
            {
                string[] values = compareCookie.Values.GetValues(mcId);
                if (values != null && values.Length > 0)
                {
                    foreach (string productId in values)
                    {
                        int prodId = Int32.Parse(productId);
                        if (!productIds.Contains(prodId))
                            productIds.Add(prodId);
                    }
                }
            }

            return productIds;
        }

        /// <summary>
        /// Returns amount of products to compare.
        /// </summary>
        /// <param name="mcId">Id of a metaClass.</param>
        /// <returns></returns>
        public static int GetCompareProductsAmount(string mcId)
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null)
            {
                string[] values = compareCookie.Values.GetValues(mcId);
                return values == null ? 0 : values.Length;
            }
            else
                return 0;
        }

        /// <summary>
        /// Returns metaclasses of products added to compare.
        /// </summary>
        /// <returns></returns>
        public static string[] GetCompareMetaClasses()
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null)
                return compareCookie.Values.AllKeys;
            return null;
        }

        /// <summary>
        /// Returns a collection of key/value pairs that are contained within a compare cookie
        /// </summary>
        /// <returns></returns>
        public static NameValueCollection GetCompareCollection()
        {
            HttpCookie compareCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(_CompareCookieItemsName));
            if (compareCookie != null)
                return compareCookie.Values;
            return null;
        }

        /// <summary>
        /// Creates app-specific cookie name.
        /// </summary>
        /// <returns></returns>
        public static string MakeCurrentApplicationCookieName(string baseName)
        {
            string path = HttpRuntime.AppDomainAppVirtualPath;
            if (!String.IsNullOrEmpty(path))
                return baseName + path.Replace('/', '_');
            else
                return baseName;
        }
        #endregion

        #region Authentication Cookies
        /// <summary>
        /// Creates the authentication cookie.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="remember">if set to <c>true</c> [remember].</param>
        /// <returns></returns>
        public static void CreateAuthenticationCookie(string username, bool remember)
        {
            // this line is needed for cookieless authentication
            FormsAuthentication.SetAuthCookie(username, remember);
            //return;

            // the code below does not work for cookieless authentication

            // we need to handle ticket ourselves since we need to save session paremeters as well
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2,
                    username,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(HttpContext.Current.Session.Timeout),
                    remember,
                    FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;

            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Secure = FormsAuthentication.RequireSSL;
            if (FormsAuthentication.CookieDomain != null)
                cookie.Domain = FormsAuthentication.CookieDomain;

            if (ticket.IsPersistent)
                cookie.Expires = ticket.Expiration;

            // Create the cookie.
            HttpContext.Current.Response.Cookies.Set(cookie);
        }
        #endregion

        #region Cookie Management
        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <param name="expires">The expiration time.</param>
        public static void SetCookie(string key, string val, DateTime expires)
        {
            HttpCookie httpCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(key));

            if (httpCookie == null)
                httpCookie = new HttpCookie(MakeCurrentApplicationCookieName(key));

            // Set cookie value
            httpCookie.Value = val;
            httpCookie.Expires = expires;

            HttpContext.Current.Response.Cookies.Set(httpCookie);
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="expires">The expires.</param>
        public static void SetCookie(string key, NameValueCollection values, DateTime expires)
        {
            HttpCookie httpCookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(key));

            if (httpCookie == null)
            {
                httpCookie = new HttpCookie(MakeCurrentApplicationCookieName(key));
            }
            // Set cookie value
            httpCookie.Values.Clear();
            httpCookie.Values.Add(values);

            httpCookie.Expires = expires;
            HttpContext.Current.Response.Cookies.Set(httpCookie);
        }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static NameValueCollection GetCookie(string key)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            string cookieName = MakeCurrentApplicationCookieName(key);
            HttpCookie cookie = null;

            foreach (string cookieKey in HttpContext.Current.Response.Cookies.Keys)
            {
                if (cookieName.Equals(cookieKey, StringComparison.OrdinalIgnoreCase))
                    cookie = HttpContext.Current.Response.Cookies.Get(cookieName);
            }

            if (cookie != null)
                return cookie.Values;

            cookie = HttpContext.Current.Request.Cookies.Get(cookieName);
            if (cookie != null)
                return cookie.Values;

            return null;
        }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetCookieValue(string key)
        {
            string cookieName = MakeCurrentApplicationCookieName(key);
            string val = null;

            if (HttpContext.Current.Request.Cookies[cookieName] != null)
                val = HttpContext.Current.Request.Cookies[cookieName].Value;

            return val;
        }

        /// <summary>
        /// Clears the cookie.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void ClearCookie(string key, string value)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(MakeCurrentApplicationCookieName(key));
            if (cookie != null)
            {
                if (String.IsNullOrEmpty(value))
                {
                    cookie.Values.Clear();
                    cookie.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Set(cookie);
                }
                else
                {
                    string[] values = cookie.Values.GetValues(value);
                    if (values != null && values.Length > 0)
                    {
                        cookie.Values.Remove(value);
                        cookie.Expires = DateTime.Now.AddYears(-1);
                        HttpContext.Current.Response.Cookies.Set(cookie);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Returns converted datetime value based on current user's settings.
        /// </summary>
        /// <param name="dt">DateTime in UTC.</param>
        /// <returns></returns>
        public static DateTime GetUserDateTime(DateTime dt)
        {
            // TODO: need to store time zone settings for each user and display datetime based on these settings.
            return dt.ToLocalTime();
        }

        #region QueryString Helpers
        /// <summary>
        /// Gets the value from query string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetValueFromQueryString(string name, string defaultValue)
        {
            string val = HttpContext.Current.Request.QueryString[name];

            if (String.IsNullOrEmpty(val))
                val = HttpContext.Current.Request.Form[name];

            return !String.IsNullOrEmpty(val) ? val : defaultValue;
        }
        #endregion


        public static string GetMoneyString(decimal amount, string currency)
        {
            return MoneyFromAmount(amount, currency).ToString();
        }

        private static Money MoneyFromAmount(decimal amount, string currency)
        {
            Currency crcy = new Currency(currency);
            return new Money(amount, crcy);
        }
    }

    public class EntryGroupCollection
    {
        private string _MetaClassName = String.Empty;
        private string _MetaClassFriendlyName;
        private List<Entry> _Entries;

        public string MetaClassName
        {
            get
            {
                return _MetaClassName;
            }
            set
            {
                _MetaClassName = value;
            }
        }

        public string MetaClassFriendlyName
        {
            get
            {
                return _MetaClassFriendlyName;
            }
            set
            {
                _MetaClassFriendlyName = value;
            }
        }

        public List<Entry> Entries
        {
            get
            {
                return _Entries;
            }
            set
            {
                _Entries = value;
            }
        }
    }
}
using EPiServer.Integration.Client.Models.Catalog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Service_api_mvc_client.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace Service_api_mvc_client.Controllers
{
    public class HomeController : Controller
    {
        public HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44328/");
            var fields = new Dictionary<string, string>
                    {
                        { "grant_type", "password" },
                        { "username", "admin" },
                        { "password", "store" }
                    };
            var response = client.PostAsync("/episerverapi/token", new FormUrlEncodedContent(fields)).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                var adminToken = JObject.Parse(content).GetValue("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken.ToString());
            }
            return client;
        }

        public ActionResult Index()
        {
            HttpClient client = GetHttpClient();

            var result = client.GetAsync("/episerverapi/commerce/entries/0/50").Result.Content.ReadAsStringAsync().Result;

            Entries entries = JsonConvert.DeserializeObject<Entries>(result);
            
            return View(entries);
        }

        public ActionResult SingleEntry(string Code)
        {
            EntryViewModel viewModel = new EntryViewModel();

            HttpClient client = GetHttpClient();

            var result = client.GetAsync($"/episerverapi/commerce/entries/{Code}").Result.Content.ReadAsStringAsync().Result;
            viewModel.SelectedEntry = JsonConvert.DeserializeObject<Entry>(result);

            var priceResult = client.GetAsync($"/episerverapi/commerce/entries/{Code}/prices").Result.Content.ReadAsStringAsync().Result;
            viewModel.Prices = JsonConvert.DeserializeObject<IEnumerable<Price>>(priceResult);

            return View(viewModel);
        }
    }
}
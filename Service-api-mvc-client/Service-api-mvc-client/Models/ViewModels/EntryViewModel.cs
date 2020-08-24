using EPiServer.Integration.Client.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service_api_mvc_client.Models.ViewModels
{
    public class EntryViewModel
    {
        public Entry SelectedEntry { get; set; }
        public IEnumerable<Price> Prices { get; set; }
    }
}
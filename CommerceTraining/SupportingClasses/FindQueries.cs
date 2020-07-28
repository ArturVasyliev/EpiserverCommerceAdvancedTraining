using CommerceTraining.Models.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Globalization;
using Mediachase.Commerce.Orders;
using SpecialFindClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EPiServer.Find.Cms; // ...for .GetContentResult(); 
using EPiServer.Find.Commerce; // extensions are here
using Mediachase.Commerce;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Commerce.Order;


namespace CommerceTraining.SupportingClasses
{

    public class FindQueries
    {

        private readonly IClient _client;
        public bool WentWell { get; set; }

        // target for typed query
        public List<ShirtVariation> variantList = new List<ShirtVariation>();
        public List<ShirtProduct> productList = new List<ShirtProduct>();

        // no demo, just checking
        public void temp()
        {
            //  _client.Conventions.IdConvention
            //.ForInstancesOf<OrderValues>()
            //.IdIs(x => x.SocialSecurityNumber);

            //ContentIndexer.Instance.Conventions.ShouldIndexConvention(x => false);//. 
            //.ForInstancesOf<SomeModel>().ShouldIndex(x => false);

            var searchResults = _client.Search<AddressValues>()
                .TermsFacetFor(x => x.CityName)
                .Filter(x => x.CountryName.Match("Germany"))
                .Take(0)
                .GetResult();


        }

        #region .ctor(s) taking the client

        public FindQueries()
        {
            _client = Client.CreateFromConfig();
        }

        // Different kind if "Client"
        public FindQueries(IClient client) // integrated
        {
            _client = client;
        }

        public FindQueries(IClient client, bool native) // native
        {
            _client = client;
        }

        #endregion

        #region Native mode

        // Demo
        public void GetNative(string keyWords)
        {
            // not integrated, may need projection
            var result = _client.Search<FashionNode>() // note: a node
                .For(keyWords)
                .Select(x => new // doing like this... for the Dictionary
                {
                    //MainBody = x.MainBody, // crash expected
                    //MainBody = x.MainBody.AsCropped(200), // still error, no deserializer
                    Name = x.Name,
                    Code = x.Code,
                    //Contentlink = x.ContentLink // ...also error for this one 
                }
                   )
                   .GetResult();
        }

        #endregion

        #region Integrated mode

        // Demo - need a clean-up
        public Dictionary<string, string> GetIntegrated(string keyWords)
        {
            Dictionary<string, string> strings = new Dictionary<string, string>();

            //langList.Add("en"); // old
            //langList.Add(ContentLanguage.PreferredCulture.Name); // not used
            //Language lang = new Language(ContentLanguage.PreferredCulture.Name, null, null); // not used

            var result = SearchClient.Instance.Search<ShirtVariation>() // Check if this applies
              .For(keyWords)
              //.Filter(x=> x.Margin.GreaterThan(20)) 
              .Filter(x => x.Margin.InRange(20, 40)) // ... at the Variation only // Margin added to model for Adv.
              .TermsFacetFor(x => x.Brand)
              .TermsFacetFor(x => x.Size)
              .GetContentResult(); // SDK states this... SDK needs an update, stuff (params) has changed

            // ...gone
            //.FilterOnLanguages(langList) // 
            //.GetContentResult(); // in 8... GetContentResult(new LanguageSelector("en"));

            this.variantList = result.OfType<ShirtVariation>().ToList(); // ...seems like "new style"

            var hits = result.SearchResult.Hits; //... 
            //var hits = result.Hits; // old/native

            strings.Add(result.SearchResult.Hits.Count().ToString(), " hits");
            //strings.Add(result.Hits.Count().ToString(), " hits");// ...seems like new style

            // could do like this and then the "loader" if duplicated index entries (will be remedied)
            // else .Exist() or ExistKey() ... or below
            var distinct = hits.Select(h => h.Document.ContentLink).Distinct();

            foreach (var item in distinct)
            {
                strings.Add("Distinct itemID: " + item.ID.ToString(), "");
            }

            foreach (var item in result.TermsFacetFor(x => x.Brand))
            {
                strings.Add("Brand: " + item.Term, item.Count.ToString()); // EPiServer.Find.Api.Facets.TermCoun
            }

            foreach (var item in result.TermsFacetFor(x => x.Size))
            {
                strings.Add("Size" + item.Term, item.Count.ToString());
            }

            return strings;
        }

        #endregion

        #region Lab/Demo - Native mode with PO-X-tras

        #region Checking on indexing the OrderAddress collection

        // native
        private void IndexAddressesSeparately(PurchaseOrder order)
        {
            // may need a check here, do like this for now
            IndexSeveralObjects(order);
        }

        // native
        private void IndexSeveralObjects(PurchaseOrder order)
        {
            // local variables
            IEnumerable<AddressValues> addressValues = null;
            OrderValues orderValues = null;

            addressValues = SplitOrder(order);
            orderValues = AggregateValues(order);

            IndexInFind(orderValues, addressValues);
        }

        // trying to index the whole address - not good - error
        public void IndexWholeAddressCollection(PurchaseOrder order)
        {
            IEnumerable<OrderAddress> addresses = order.OrderAddresses;
            foreach (OrderAddress address in addresses)
            {
                _client.Index(address); // error
                /*Self referencing loop detected for property 'ManifestModule' with type 'System.Reflection.RuntimeModule'. 
                 * Path 'Parent.ExchangeOrderNumberMethod.Method.Module.Assembly'.*/
            }
        }

        #endregion

        // native - have this in "starters"
        // Done for lab
        public void OrderForFind(IPurchaseOrder order)
        {
            // RoCe: Fix this with PurchaseOrder vs. IPurchaseOrder
            IndexInFind(AggregateValues(order as PurchaseOrder), SplitOrder(order as PurchaseOrder)); // Prepare for adding objects to Find
        }

        // Done for lab - Cleaned
        private void IndexInFind(OrderValues orderValues, IEnumerable<AddressValues> addressValues)
        {
            // ...just a test with several addesses, as we in the checkout only have one address
            List<AddressValues> locallist = addressValues.ToList();
            AddressValues v = new AddressValues
            {
                CityName = "Motala",
                CountryName = "Tibet",
                ID = "DummyAddress",
                Line1 = "SmallTown",
                OrderGroupAddressId = -1,
                OrderGroupId = addressValues.FirstOrDefault().OrderGroupId // borrowing some
            };

            locallist.Add(v);

            foreach (AddressValues item in addressValues)
            {
                // can get this - "Invalid type id. Maximum 200 word characters (letter, number, underscore, dot, comma)
                item.ID = item.ID.Replace(" ", "-"); // do not like whitespace
            }

            if (addressValues == null) // if it of some reason is
            {
                try
                {
                    _client.Index(orderValues);
                    WentWell = true;
                }
                catch (Exception)
                {
                    WentWell = false;
                }
            }
            else
            {
                try
                {
                    _client.Index(orderValues); // gets one doc with IEnumerables
                    _client.Index(locallist); // gets 2 docs
                    WentWell = true;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    string inner = ex.InnerException.Message;
                    WentWell = false;
                }
            }
        }

        // Done for lab - Cleaned
        private OrderValues AggregateValues(PurchaseOrder order)
        {
            // Do the below, else possible null-ex
            string custName = String.Empty;
            if (String.IsNullOrEmpty(order.CustomerName))
            { custName = "Anonymous"; }
            else { custName = order.CustomerName; }

            Dictionary<string, decimal> itemDict = new Dictionary<string, decimal>(); // target for LineItems & Qty
            foreach (LineItem item in order.OrderForms[0].LineItems)
            {
                itemDict.Add(item.Code, item.Quantity);
            }

            // using a single OrderForm in this example
            OrderValues po = new OrderValues()
            {
                lineItems = itemDict,
                lineItemTotal = order.OrderForms[0].LineItems.Sum(x => x.PlacedPrice * x.Quantity),  // should consider discounts
                PoTrackingNo = order.TrackingNumber,
                LineItemCodes = order.OrderForms[0].LineItems.Select(l => l.Code).ToList(),
                SubTotal = order.SubTotal,
                PoType = order.MetaClass.Name,
                orderDate = order.Created,
                orderGroupId = order.OrderGroupId,
                customerName = custName,
                currency = order.BillingCurrency,
            };

            return po;
        }

        // Done for lab - Cleaned
        private IEnumerable<AddressValues> SplitOrder(PurchaseOrder order)
        {
            // ToDo: Cut out adresses and treat separately
            // the rest will be indexed as usual (indexing the address collection throws an Ex.)
            List<OrderAddress> orderAddresses = new List<OrderAddress>(order.OrderAddresses); // source
            List<AddressValues> orderValues = new List<AddressValues>(); // target

            foreach (OrderAddress address in orderAddresses)
            {
                // Could add the AddressType field to the OrderAddress, exist on the customer address (BF)
                if (true/*address["AddressType"].ToString() == "Shipping"*/)
                {
                    AddressValues addressValues = new AddressValues()
                    {
                        // can have spaces ... not good for ID, calls for cleaning
                        ID = address.Name + order.TrackingNumber,
                        CountryName = address.CountryName,
                        CityName = address.City,
                        Line1 = address.Line1,
                        OrderGroupAddressId = address.OrderGroupAddressId,
                        OrderGroupId = address.OrderGroupId
                    };

                    orderValues.Add(addressValues);
                }
            }
            return orderValues;
        }

        // for the lab in VariationController - Cleaned
        public IEnumerable<string> GetItems(string entryCode)
        {
            List<string> localList = new List<string>(); // included in starters

            #region DateFilter for the query - need new one, check in cmd-app
            // bad date-filter,  - 
            //.Filter(f=> f.orderDate.InRange(DateTime.UtcNow,DateTime.UtcNow.AddMonths(-1)))
            //                .FilterHits(f=>f.LineItemCodes.)
            #endregion

            // something has changed in Find, no hits if done as before
            var result = _client.Search<OrderValues>() // could be just the items in a separate class --> more precise
                .For(entryCode)
                //.InField("LineItemCodes")
                .GetResult();

            // can do smarter, but it´s explicit :=)
            foreach (var item in result)
            {
                foreach (var item2 in item.LineItemCodes)
                {
                    if (item2 != entryCode) // excluding what was searched for
                    {
                        if (localList.Contains(item2))
                        {
                            // ...do something else
                        }
                        else // add it
                        {
                            localList.Add(item2);
                        }
                    }
                }
            }

            return localList;
        }

        #region Test-NoDemo

        // native
        public IEnumerable<OrderValues> JustChecking()
        {
            return _client.Search<OrderValues>()
               .For("Long-Sleeve-Shirt-White-Small_1")
               .InField("LineItemCodes")
               .GetResult();
        }


        // native
        public void IndexForTest()
        {
            SpecialFindClasses.FindTestClass thing = new SpecialFindClasses.FindTestClass
            {
                theID = 2,
                theString = "Some String for number two",
                theOtherString = "Some Other String for number two"
            };

            try
            {
                _client.Index(thing); // ...Find was down, works now
            }
            catch (Exception ex)
            {
                string e = ex.Message;

            }
        }

        #endregion

        #endregion

        #region New Stuff for ECF-Find in 9.2 - 9.3 - Demo some

        public void NewFindMethods(VariationContent variationContent)
        {
            #region FindStuff - need to check this


            var p = GetProductsForVariation(variationContent); // okay
            // ?? var v1 = GetAssociations("Accessories",variationContent.Code); // fick inget 

            // new style, custom Find-q
            GetAssociations("Accessories", variationContent.Code); // fortfarande Json-knas

            // This should give hits on the blue shirt, but it didn't
            //var v2 = GetAssociations("CrossSell"); // fick inget 
            //var v3 = GetAssociations("NiceToHave"); // fick inget ... trots att det är den "TypeId" som finns


            //var v4 = GetAssociations("Cool"); // fanns i index, men paj på json Conr-ref
            // GetAssociations("Cool",variationContent.Code); //... trying

            GetVariationReferencesWithInventories(); // seems ok now after some plumbing
            #endregion
            var p1 = GetVariationReferencesWithUnitPrice(MarketId.Default); // error
            //var a = GetAncestors(variationContent); // eget

            //var p0 = GetVariationReferencesWithPrices();
            // nope, get this ... again... Cannot deserialize the current JSON object (e.g. {"name":"value"}) into type 
            //'EPiServer.Find.Cms.IndexableContentReference' because the type requires a JSON string value to deserialize correctly.

            //TestSomeStuff(variationContent);
        }

        private void TestSomeStuff(VariationContent variationContent)
        {
            SearchClient.Instance.Search<IContent>() // 
                .GetContentResult();//.GetContentResult();


            SearchClient.Instance.Search<PageData>() // 
            .GetPagesResult(); // ...?


            // SearchClient.Instance.Search<ImageData>() // integrated
            //     .GetFilesResult(); //...?

            //SearchRequestExtensions.

        }

        public void NewExtensionMethods(ProductContent productContent)
        {
            //var r = GetEntriesByMarket(MarketId.Default); // okay
            //var v = GetVariationsForProduct(productContent); // error
        }

        public void NewExtensionMethods(NodeContent nodeContent)
        {
            //var r = GetEntriesByMarket(MarketId.Default); // okay
            //var v = GetParentNodes(nodeContent); // okay
            //var n = GetChildNodes(nodeContent); // okay

        }

        // not checked...
        private object GetAncestors(VariationContent variationContent)
        {
            return SearchClient.Instance.Search<ShirtVariation>()
                //.Filter(x=>x.)
                .GetContentResult();
        }

        private IEnumerable<ProductContent> GetProductsForVariation(VariationContent variation) // ok
        {
            //return _client.Search<ProductContent>() // native
            return SearchClient.Instance.Search<ShirtProduct>() // integrated
                                                                //.Filter(x=> x.)
                .Filter(x => x.Variations().MatchContained(y => y.ID, variation.ContentLink.ID))
                //.Filter(x=>x.)
                .GetContentResult();
        }

        // ??
        private IEnumerable<ContentReference> GetVariationsForProduct(ProductContent productContent)
        {
            var result = _client.Search<ProductContent>()
           //return _client.Search<ProductContent>()
           //return SearchClient.Instance.Search<ProductContent>()
           .Filter(x => x.ContentLink.Match(productContent.ContentLink))
           .Select(x => x.Variations())
                //.GetContentResult();
                .GetResult();

            //.SingleOrDefault();

            return null;  // for now

            // ShirtProduct & Integrated Client -->Error: Sequence contains more than one element
            // ...with native client --> 
            /*Cannot deserialize the current JSON object (e.g. {"name":"value"}) into type 'EPiServer.Core.ContentReference' because the type requires a JSON string value to deserialize correctly.
To fix this error either change the JSON to a JSON string value or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object.
Path 'VariantsReference.___types', line 1, position 881*/

        }

        private IEnumerable<NodeContent> GetChildNodes(NodeContent parentNode) // ok
        {
            //return _client.Search<NodeContent>()
            return SearchClient.Instance.Search<NodeContent>()
                .Filter(x => x.ParentNodeRelations().MatchContained(c => c.ID, parentNode.ContentLink.ID))
                .GetContentResult();
        }

        private IEnumerable<NodeContent> GetParentNodes(NodeContent childNode) // ok
        {
            //return _client.Search<NodeContent>()
            return SearchClient.Instance.Search<NodeContent>()
                .Filter(x => x.ChildNodeRelations().MatchContained(c => c.ID, childNode.ContentLink.ID))
                .GetContentResult();
        }

        // not tested
        private IEnumerable<EntryContentBase> GetBundleEntries(BundleContent bundleContent)
        {
            return _client.Search<EntryContentBase>()
                .Filter(c => c.ParentBundles().MatchContained(b => b.ID, bundleContent.ContentLink.ID))
                .GetContentResult();
        }

        private IEnumerable<BundleContent> GetBundlesForEntry(EntryContentBase entryContentBase)
        {
            return _client.Search<BundleContent>()
                .Filter(c => c.BundleEntries().MatchContained(b => b.ID, entryContentBase.ContentLink.ID))
                .GetContentResult();
        }

        private IEnumerable<EntryContentBase> GetPackageEntries(PackageContent packageContent)
        {
            return _client.Search<EntryContentBase>()
                .Filter(c => c.ParentPackages().MatchContained(b => b.ID, packageContent.ContentLink.ID))
                .GetContentResult();
        }

        private IEnumerable<PackageContent> GetPackagesForEntry(EntryContentBase entryContentBase)
        {
            return _client.Search<PackageContent>()
                .Filter(c => c.PackageEntries().MatchContained(b => b.ID, entryContentBase.ContentLink.ID))
                .GetContentResult();
        }

        //private IDictionary<ContentReference, IEnumerable<ContentReference>> GetAssociations(string type) // no hits or json error
        private void GetAssociations(string type, string code)
        {
            // SearchClient.Instance.
            var xx = SearchClient.Instance
                .Search<VariationContent>(Language.Swedish)
                .Filter(c => c.Associations().MatchContained(x => x.Type, type))
                //.Filter(cc=> cc.)
                //.Select(x => new { x.ContentLink, Associations = x.Associations() })
                .GetContentResult();
            //.ToDictionary(
            //k => k.ContentLink,
            //v => v.Associations.SelectMany(x => x.List));


            //_client.Search<VariationContent>()
            //   .Filter(f => f.Code.Match(code))
            //   .Select(s => new
            //   {
            //       code = s.Code,
            //       //a = s.Associations().SelectMany(x=>x.List)
            //       // a = s.Associations().Select(x=> x.Type)

            //   })
            //   .GetResult();

        }

        private IEnumerable<EntryContentBase> GetEntriesByMarket(MarketId marketId) // ok
        {
            return _client.Search<EntryContentBase>()
                .Filter(c => c.Markets().MatchContained(x => x.Value, marketId.Value))
                .GetContentResult();
        }

        // nope
        private IEnumerable<dynamic> GetVariationReferencesWithUnitPrice(MarketId marketId)
        {
            return _client.Search<VariationContent>()
                .Filter(c => c.DefaultPrice().MarketId.Match(marketId))
                .Filter(x => x.DefaultPrice().UnitPrice.InRange(10, 200, Currency.USD))
                //.Filter(x => x.DefaultPrice().UnitPrice.Match(new Money(10, Currency.USD)))
                //.Filter(x => x.DefaultPrice().UnitPrice.LessThan(new Money(10, Currency.USD)))
                //.Filter(x => x.DefaultPrice().UnitPrice.GreaterThan(new Money(10, Currency.USD)))
                //.Filter(x => x.DefaultPrice().UnitPrice.Currency.Match(Currency.USD))
                .Select(x => new
                {
                    x.ContentLink, // does not

                    DefaultPrice = x.DefaultPrice().UnitPrice
                })
                .GetResult();
            // Got --> Cannot deserialize the current JSON object (e.g. {"name":"value"}) into type 'EPiServer.Find.Cms.IndexableContentReference'
        }

        // Json-error
        private IEnumerable<dynamic> GetVariationReferencesWithPrices()
        {
            return _client.Search<VariationContent>()
                .Select(x => new
                {
                    //x.ContentLink, // didn't help
                    Prices = x.Prices()
                })
                .GetResult();
        }

        // ... working, after some hassle
        private void GetVariationReferencesWithInventories() // changed to void
        {
            var r = SearchClient.Instance.Search<VariationContent>(Language.English) // changed the client, Lang. did not help
                .Select(x => new
                {
                    link = x.ContentLink,
                    Inventories = x.Inventories()
                })
                .GetResult();

            // added for clarity & avoiding errors
            List<Inventory> ii = new List<Inventory>();
            List<ContentReference> rr = new List<ContentReference>();
            Dictionary<ContentReference, List<Inventory>> dict = new Dictionary<ContentReference, List<Inventory>>();
            foreach (var item in r)
            {
                foreach (var item2 in item.Inventories)
                {
                    ii.Add(item2);
                }

                if (dict.ContainsKey(new ContentReference(item.link.ID)))
                {
                    // do nothing
                }
                else
                {
                    dict.Add(new ContentReference(item.link.ID), ii);
                    ii.Clear();
                }
            }
        }

        // not checked
        private IEnumerable<dynamic> GetContentReferencesWithImageUrls()
        {
            return _client.Search<EntryContentBase>()
                .Select(x => new
                {
                    x.ContentLink,
                    ImageUrl = x.DefaultImageUrl(),
                    ThumbnailUrl = x.ThumbnailUrl()
                })
                .GetResult();
        }


        //*/
        #endregion

        #region SDK-examples

        // Pricing & Inventory
        public void SDKExamples(ContentReference contentReference)
        {
            // Pricing
            Currency currency = new Currency("USD");

            var result = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.DefaultPrice().UnitPrice.Currency.Match(currency))
                .GetContentResult(); // Get.. is missing in SDK

            var result2 = SearchClient.Instance.Search<VariationContent>()
                 .Filter(x => x.DefaultPrice().UnitPrice.LessThan(new Money(200M, new Currency("USD"))))
                 .GetContentResult(); // rewritten, wrong args in SDK

            var result3 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.DefaultPrice().UnitPrice.InRange(50, 150, currency))
                .GetContentResult(); // rewritten, wrong args in SDK

            var result4 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.InStockQuantityLessThan(200))
                .GetContentResult();

            // ...MatchInventoryStatus is obsolete
            //var result5 = SearchClient.Instance.Search<VariationContent>()
            //    .Filter(x => x.MatchInventoryStatus(InventoryStatus.Enabled)) 
            //    .GetContentResult();

            var result5 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.MatchIsTracked(true))
                .GetContentResult();

            var result6 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.MatchWarehouseCode("Stockholm"))
                .GetContentResult();
        }

        // nothing yet
        public void SDKExamples(ProductContent prod)
        {


        }


        #endregion
    }

}
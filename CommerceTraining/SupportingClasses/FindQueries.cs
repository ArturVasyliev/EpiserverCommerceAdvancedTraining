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
//EPiServer.Find.Commerce.CatalogContentBaseExtensions;
//EPiServer.Find.Commerce.ProductContentExtensions;
using EPiServer.Find.Commerce; // extensions are here
using Mediachase.Commerce;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Find.Cms;
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
        public void TempNoDemo()
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


        #region .ctor(s) taking the client

        //public FindQueries()
        //{
        //    _client = Client.CreateFromConfig();
        //}

        // Different kind of "Client"(s)
        public FindQueries(IClient client) // integrated
        {
            _client = client;
        }

        public FindQueries(IClient client, bool native) // native
        {
            _client = client;
        }

        #endregion

        #region Integrated mode

        // Demo - need a clean-up
        public Dictionary<string, string> GetIntegrated(string keyWords)
        {
            Dictionary<string, string> strings = new Dictionary<string, string>();

            //langList.Add(ContentLanguage.PreferredCulture.Name); // not used
            //Language lang = new Language(ContentLanguage.PreferredCulture.Name, null, null); // not used

            var result = _client.Search<ShirtVariation>() // Check if this applies
              .For(keyWords)
              //.Filter(x=> x.Margin.GreaterThan(20)) //  Custom field
              .Filter(x => x.Margin.InRange(20, 40)) // ... at the Variation only // Margin added to model for Adv.
              .TermsFacetFor(x => x.Brand)
              .TermsFacetFor(x => x.Size)
              .GetContentResult(); // SDK states this... params has changed

            // ...gone
            //.FilterOnLanguages(langList) // 
            // "new style"
            // this.variantList = result.OfType<ShirtVariation>().ToList(); 

            var hits = result.SearchResult.Hits; //... 
            //var hits = result.Hits; // old/native

            strings.Add(result.SearchResult.Hits.Count().ToString(), " hits");

            // could do like this and then the "loader" if duplicated index entries (will be remedied)
            // else .Exist() or ExistKey() ... or below
            var distinct = hits.Select(h => h.Document.ContentLink).Distinct();

            foreach (var item in distinct)
            {
                strings.Add("Distinct itemID: " + item.ID.ToString(), "");
            }

            foreach (var item in result.TermsFacetFor(x => x.Brand))
            {
                strings.Add("Brand: " + item.Term, item.Count.ToString()); // EPiServer.Find.Api.Facets.TermCount
            }

            foreach (var item in result.TermsFacetFor(x => x.Size))
            {
                strings.Add("Size" + item.Term, item.Count.ToString());
            }

            return strings;
        }

        #endregion

        #region Native mode

        public void GetNative(string keyWords)
        {
            // not integrated, need "projection"
            var result = _client.Search<FashionNode>(Language.Swedish) // ... a node - only Swedish
                .For(keyWords)
                .InField(x => x.MainBody) // not using the "All" field... when doing like this
                .Select(x => new 
                {
                    // .MainBody, // crash expected, no deserializer
                    // x.ContentLink // ...error too
                    x.Name,
                    x.Code,
                }
                )
                .GetResult();
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

            if (addressValues == null) // if it of some reason should be
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

            // target for LineItems & Qty
            Dictionary<string, decimal> itemDict = new Dictionary<string, decimal>();

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

        #region Stuff for ECF+Find - Demo some

        #region NoDemo

        public void NewFindMethods(VariationContent variationContent)
        {
            #region FindStuff - need to check this


            var p = GetProductsForVariation(variationContent.ContentLink); // okay
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

        // not checked... fishy
        private object GetAncestors(VariationContent variationContent)
        {
            return SearchClient.Instance.Search<ShirtVariation>()
                //.For(x=>x.Ancestors)
                //.Filter(An)
                .GetContentResult();
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

        #endregion

        #region Products - Variations

        private IEnumerable<ProductContent> GetProductsForVariation(ContentReference variationContentRef) // ok
        {
            var result = SearchClient.Instance.Search<ShirtProduct>() // integrated
                .Filter(x => x.Variations()
                .MatchContained(y => y.ID, variationContentRef.ID))
                .GetContentResult();

            return null; // for now
        }

        // ??
        private IEnumerable<ContentReference> GetVariationsForProduct(ContentReference productContentRef)
        {
            var result = _client.Search<ProductContent>()
           //return _client.Search<ProductContent>()
           //return SearchClient.Instance.Search<ProductContent>()
           .Filter(x => x.ContentLink.Match(productContentRef))
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

        public IEnumerable<EntryContentBase> GetEntriesByMarket(MarketId marketId) // ok
        {
            var result = _client.Search<EntryContentBase>()
                .Filter(c => c.Markets().MatchContained(x => x.Value, marketId.Value))
                .GetContentResult();

            return null; // for now
        }

        #endregion

        #region Nodes

        private IEnumerable<NodeContent> GetChildNodes(ContentReference parentNodeRef) // ok
        {
            //return _client.Search<NodeContent>()
            var result = SearchClient.Instance.Search<NodeContent>()
                .Filter(x => x.ParentNodeRelations().MatchContained(
                    c => c.ID, parentNodeRef.ID))
                .GetContentResult();

            return null; // for now
        }

        private IEnumerable<NodeContent> GetParentNodes(ContentReference childNodeRef) // ok
        {
            //return _client.Search<NodeContent>()
            var result = SearchClient.Instance.Search<NodeContent>()
                .Filter(x => x.ChildNodeRelations().MatchContained(
                    c => c.ID, childNodeRef.ID))
                .GetContentResult();

            return null; // for now
        }

        #endregion

        #region Package-Bundle

        // not tested
        private IEnumerable<EntryContentBase> GetBundleEntries(ContentReference bundleContentRef)
        {
            var result = _client.Search<EntryContentBase>()
                .Filter(c => c.ParentBundles().MatchContained(b => b.ID, bundleContentRef.ID))
                .GetContentResult();

            return null; // for now
        }

        private IEnumerable<BundleContent> GetBundlesForEntry(ContentReference bundleContentRef)
        {
            var result = _client.Search<BundleContent>()
                .Filter(c => c.BundleEntries().MatchContained(b => b.ID, bundleContentRef.ID))
                .GetContentResult();

            return null; // for now
        }

        private IEnumerable<EntryContentBase> GetPackageEntries(ContentReference packageContentRef)
        {
            var result = _client.Search<EntryContentBase>()
                .Filter(c => c.ParentPackages().MatchContained(
                    b => b.ID, packageContentRef.ID))
                .GetContentResult();

            return null; // for now
        }

        private IEnumerable<PackageContent> GetPackagesForEntry(ContentReference entryRef)
        {
            var result = _client.Search<PackageContent>()
                .Filter(c => c.PackageEntries().MatchContained(b => b.ID, entryRef.ID))
                .GetContentResult();

            return null; // for now
        }


        #endregion

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

        #region HaveThisElseWhere

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
        
        #endregion

        public void VariationExamples(ContentReference variationRef)
        {
           GetProductsForVariation(variationRef);

        }

        public void ProductExamples(ContentReference productRef)
        {
            GetVariationsForProduct(productRef);
        }

        public void NodeExamples(ContentReference nodeRef)
        {
            GetChildNodes(nodeRef);
            GetParentNodes(nodeRef);
        }

        public void BundleExamples(ContentReference entryContentRef)
        {
            //GetPackageEntries(entryContentRef);
            GetBundleEntries(entryContentRef);
            //GetPackagesForEntry(entryContentRef);
            GetBundlesForEntry(entryContentRef);
        }

        public void PackageExamples(ContentReference entryContentRef)
        {
            GetPackageEntries(entryContentRef);
            //GetBundleEntries(entryContentRef);
            GetPackagesForEntry(entryContentRef);
            //GetBundlesForEntry(entryContentRef);
        }


        //*/
        #endregion

        #region SDK-examples

        // Pricing & Inventory
        public void SDKExamples(ContentReference contentReference)
        {
            // Pricing - Inventory
            Currency currency = new Currency("USD");

            var result1 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.DefaultPrice().UnitPrice.Currency.Match(currency))
                .GetContentResult(); // 

            var result2 = SearchClient.Instance.Search<VariationContent>()
                 .Filter(x => x.DefaultPrice().UnitPrice.LessThan(new Money(200M, new Currency("USD"))))
                 .GetContentResult(); // rewritten, wrong args in SDK

            var result3 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.DefaultPrice().UnitPrice.InRange(50, 150, currency))
                .GetContentResult(); // rewritten, wrong args in SDK

            var result4 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.InStockQuantityLessThan(200))
                .GetContentResult();

            var result5 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.MatchIsTracked(true))
                .GetContentResult();

            var result6 = SearchClient.Instance.Search<VariationContent>()
                .Filter(x => x.MatchWarehouseCode("Nashua"))
                .GetContentResult();
        }

        // nothing yet
        public void SDKExamples(ProductContent prod)
        {


        }


        #endregion

        #region Extensionmethods

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

        #endregion


    }
}
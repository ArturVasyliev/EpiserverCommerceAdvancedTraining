using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Blocks;
using EPiServer.Commerce.Catalog.Linking;
using System.Web.Routing;
using CommerceTraining.Models.ViewModels;
using EPiServer.ServiceLocation;
using System.Text;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data;
using System.Data;

namespace CommerceTraining.Controllers
{
    public class RelatedStuffBlockController : BlockController<RelatedStuffBlockType>
    {
        public IEnumerable<Relation> Relations { get; set; }
        public IEnumerable<Association> Associations { get; set; }

        // the below means the CatalogContent and it´s parent node
        public ContentReference TheContentImOn { get; set; }
        public ContentReference TheNodeMyContentIsIn { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        protected override IActionInvoker CreateActionInvoker()
        {
            return base.CreateActionInvoker();
        }

        public RelatedStuffBlockController() // executes "on the site"
        {
            IEnumerable<string> assoc = GetAssociations();
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            // executes "on the site"
            return base.BeginExecute(requestContext, callback, state);
        }

        public override ActionResult Index(RelatedStuffBlockType currentBlock)
        {
            ContentReference cRef = currentBlock.TheRef; // gets null, even though it´s set in the model class
            
            ContentReference p = OtherClass.proppen; // if we can make it happen - gets the CastleProxy

            //var x =  p.;

            EntryContentBase daStuff = loader.Service.Get<EntryContentBase>(p); // gets the CastleProxy
            string s = daStuff.Code;

            RelatedStuffBlockViewModel model = new RelatedStuffBlockViewModel()
            {
                theContent = GetContentName(),
                theParentContent = GetParentContentName(),
                Name = currentBlock.Name,
                RelatingTo = currentBlock.RelatingTo, // set a dummy string, try to override in the model
                associations = GetAssociatedReferences(),
                Type = this.Type,
                Group = this.Group,
                theEntryContentBase = daStuff
            };


            return PartialView(model);
        }

        //Injected<ILinksRepository> _linksRep;
        Injected<GroupDefinitionRepository<AssociationGroupDefinition>> _AssocGroups;

        private IEnumerable<string> GetAssociations()
        {
            // have the "loader" ... also
            bool GoOldSchool = true;
            List<string> list = new List<string>(); // to store whatever found
            // _linksRep.Service.get - no listing
            // AssociationGroup g = new AssociationGroup();


            if (GoOldSchool)
            {
                // seems to lack a MC-way to list the associations available
                // it becomes SQL-oriented pretty soon
                // ugly, but works "OldSchool"
                FilterElementCollection col = new FilterElementCollection();
                using (IDataReader reader = Mediachase.BusinessFoundation.Data.DataHelper.List("CatalogAssociation"
                    , col.ToArray()))
                {
                    // need the FeatureSwitch when going for POs
                    while (reader.Read())
                    {
                        list.Add((string)reader["AssociationName"]);
                    }
                }
            }
            else
            {
                // Gets from DDS
                //var associationDefinitionRepository =
                //    ServiceLocator.Current.GetInstance<GroupDefinitionRepository<AssociationGroupDefinition>>();

                var associationDefinitionRepository = _AssocGroups.Service;
                IEnumerable<AssociationGroupDefinition> a = associationDefinitionRepository.List();

                foreach (AssociationGroupDefinition item in a)
                {
                    list.Add(item.Name);
                }
            }
            return list;
        }



        Injected<IContentLoader> loader;
        Injected<IContentRouteHelper> c_helper; // this guy saves the day
        Injected<IPageRouteHelper> p_helper;

        // the below is not really interesting for "assoc"... look at the "ContentRouteHelper-stuff"
        private void GetInfo(RelatedStuffBlockType currentBlock)
        {
            RequestContext requestContext = new RequestContext(base.HttpContext, base.RouteData);

            var parentStack = requestContext.HttpContext.Items[ContentContext.ContentContextKey] as
               Stack<ContentContext.ContentPropertiesStack>;
            // ContentReference theRef = parentStack.FirstOrDefault().ContentLink; // Gets the Block itself
        }

        // Could add a method for listing from the node (ParentContent) ... like in another block

        private string Type { get; set; } // somewhat temporary
        private string Group { get; set; } // somewhat temporary

        private IEnumerable<ContentReference> GetAssociatedReferences()
        {
            // For test
            string assocType = String.Empty;

            string assocGroup = String.Empty;
            assocGroup = "CrossSell"; // ...for now, need to find an appropriate "prop"

            ContentReference theOne = c_helper.Service.ContentLink;
            List<ContentReference> refs = new List<ContentReference>();

            var c = loader.Service.Get<VariationContent>(theOne);

            // have the extensions-NS... "up there"
            IEnumerable<Association> assoc = c.GetAssociations();
            StringBuilder strB = new StringBuilder();

            /*  AssociationType:
             *  Cool	NiceToHave
                Default	NULL
                OPTIONAL	Optional
                REQUIRED	Required
                NULL	NULL*/

            /* CatalogAssociation:
             *  2	2	CrossSell	NULL	0
                5	2	UpSell	For making a customer happier	0
                NULL	NULL	NULL	NULL	NULL*/

            /* CatalogEntryAssociation: 
             *  2	6	100	Cool
                5	6	0	Cool
                NULL	NULL	NULL	NULL*/

            if (assoc.Count() >= 1)
            {
                foreach (Association item in assoc)
                {
                    if (item.Group.Name == assocGroup)
                    {
                        refs.Add(item.Target);
                        Type = item.Type.Description;
                        Group = item.Group.Name;
                    }
                }

                // need some additional stuff 
                //Association a = assoc.FirstOrDefault(); // get the only one .. so far for test
                //strB.Append("Group-Name: " + a.Group.Name);
                //strB.Append(" ");
                //strB.Append("Type-Descr: " + a.Type.Description);
                //// there is more to get out
                return refs;
            }
            else
            {
                //strB.Append("Nothing");
                Type = "Nothing";
                refs.Add(ContentReference.SelfReference);
                return refs;
            }
        }


        // Just testing... and it is very useful
        private string GetContentName()
        {
            ContentReference theRef = c_helper.Service.ContentLink;
            this.TheContentImOn = theRef;
            return loader.Service.Get<IContent>(theRef).Name; // the variation
        }

        private string GetParentContentName()
        {
            ContentReference parentRef = c_helper.Service.Content.ParentLink;
            this.TheNodeMyContentIsIn = parentRef;
            return loader.Service.Get<IContent>(parentRef).Name; // the parent node for the variation
        }

    }
}

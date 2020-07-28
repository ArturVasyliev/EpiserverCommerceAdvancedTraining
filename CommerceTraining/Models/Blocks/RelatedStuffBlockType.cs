using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer;
using EPiServer.Web.Routing;

namespace CommerceTraining.Models.Blocks
{
    [ContentType(DisplayName = "RelatedStuffBlockType", GUID = "ccb3fa4c-f797-4d1c-ab05-5de2e9deef2b", Description = "")]
    public class RelatedStuffBlockType : BlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Name",
            Description = "Name field's description",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual String Name { get; set; }

        public virtual string RelatingTo { get; set; }

        Injected<IPageRouteHelper> p_helper;
        Injected<IContentLoader> loader;
        Injected<IContentRouteHelper> helper; // ...ContentRouteHelper obsoleted in 10 

        public RelatedStuffBlockType() // can´t do ?
        {

        }

        public override void SetValue(string index, object value)
        {
            base.SetValue(index, value);
        }

        public virtual string Dict { get; set; } 

        [UIHint(EPiServer.Commerce.UIHint.CatalogEntry)]
        public virtual ContentReference TheRef { get; set; }

    }

    
    public static class OtherClass
    {
        public static ContentReference proppen { get; set; }

    }
}
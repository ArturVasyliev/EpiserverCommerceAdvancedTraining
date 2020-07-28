using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace CommerceTraining.Models.Blocks
{
    [ContentType(DisplayName = "NodeListingBlock", GUID = "0fa322ff-66e1-4c3c-acea-11697ff1edcd", Description = "")]
    public class NodeListingBlock : BlockData
    {

        [CultureSpecific]
        [Display(
            Name = "Name",
            Description = "Name field's description",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual String Name { get; set; }

        //[UIHint(EPiServer.Commerce.UIHint.CatalogContent)]
        //public virtual ContentReference parentNode { get; set; } // why not?

        public virtual string entries { get; set; }
    }
}
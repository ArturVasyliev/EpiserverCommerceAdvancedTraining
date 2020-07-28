using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace CommerceTraining.Models.Blocks
{
    [ContentType(DisplayName = "ProductBlock", GUID = "142ec219-caed-407d-ba50-910f2998753b", Description = "Use for special offers")]
    public class ProductBlock : BlockData
    {
        public virtual string ProductName { get; set; }
        public virtual XhtmlString productDescription { get; set; }

        [UIHint(EPiServer.Commerce.UIHint.CatalogEntry)]
        public virtual ContentReference ProductReference { get; set; }
    }
}
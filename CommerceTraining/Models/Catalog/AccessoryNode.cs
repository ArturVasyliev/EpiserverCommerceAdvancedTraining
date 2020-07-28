using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName = "Accessory_Node", DisplayName = "AccessoryNode"
        , GUID = "6fe4dca3-f455-48db-b045-57f95f948bb1", Description = "")]
    [AvailableContentTypes(Include = new Type[] { typeof(ClothesAccessory) })]  
    public class AccessoryNode : NodeContent
    {
        // Don't need ISelectionFactory (Taxes) in this one
        [IncludeInDefaultSearch]
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "Only Accessories allowed in this node",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

    }
}
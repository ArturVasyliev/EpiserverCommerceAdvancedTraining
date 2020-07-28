using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName = "Fashion_Node", DisplayName = "FashionNode", GUID = "cdab7602-f681-4768-9768-afd7fa35dd09", Description = "")]
    public class FashionNode : NodeContent
    {
        
        [IncludeInDefaultSearch]
        [CultureSpecific]
        //[Tokenize]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

        // ...for the SKU to retrieve from the node
        [SelectOne(SelectionFactoryType = typeof(GetTaxesFactory))]
        public virtual string TaxCategories { get; set; }
    }

    public class GetTaxesFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata md)
        {
            CatalogTaxDto dto = CatalogTaxManager.GetTaxCategories();
            List<ISelectItem> listOfSelectItems = new List<ISelectItem>();

            foreach (var item in dto.TaxCategory)
            {
                SelectItem daItem = new SelectItem() { Text = item.Name, Value = item.TaxCategoryId };
                listOfSelectItems.Add(daItem);
            }

            return listOfSelectItems.ToArray();
        }

    }
}
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName = "Clothes_Accessory", DisplayName = "Accessory", GUID = "8c71e8fc-cee1-477f-b82b-550fab593a84", Description = "")]
    public class ClothesAccessory : DefaultVariation
    {
        // have std. ECF-Dict in the baseclass
        [SelectOne(SelectionFactoryType = typeof(GetAccessoriesFactory))]
        public virtual string AccessoryType { get; set; }
    }

    public class GetAccessoriesFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata md)
        {
            
            List<ISelectItem> listOfSelectItems = new List<ISelectItem>();

            SelectItem suspenders = new SelectItem() { Text = "Suspenders", Value = 0 };
            listOfSelectItems.Add(suspenders);

            SelectItem belts = new SelectItem() { Text = "Belts", Value = 1 };
            listOfSelectItems.Add(belts);

            SelectItem cufflinks = new SelectItem() { Text = "Cufflinks", Value = 2 };
            listOfSelectItems.Add(cufflinks);

            SelectItem scarfs = new SelectItem() { Text = "Scarfs", Value = 3 };
            listOfSelectItems.Add(scarfs);

            SelectItem Galoshes = new SelectItem() { Text = "Galoshes", Value = 4 };
            listOfSelectItems.Add(Galoshes);

            SelectItem Umbrella = new SelectItem() { Text = "Umbrella", Value = 5 };
            listOfSelectItems.Add(Umbrella);

            return listOfSelectItems.ToArray();
        }
    }
}
﻿using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using EPiServer.Commerce.Catalog.Linking;
using System.Collections.Generic;
using System.Collections;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(MetaClassName = "Shirt_Variation"
        , DisplayName = "ShirtVariation"
        , GUID = "c0058f2d-9893-41d9-8c19-19c94d34ded1"
        , Description = "Use with mens shirts")]
    public class ShirtVariation : DefaultVariation
    {
        [Searchable]
        [IncludeInDefaultSearch]
        public virtual string Size { get; set; }

        [Searchable]
        [IncludeValuesInSearchResults]
        public virtual string Color { get; set; }

        public virtual bool CanBeMonogrammed { get; set; }

        // added for adv. 
        //public virtual bool RequireSpecialShipping { get; set; }... moved into "base"

        [Searchable]
        [IncludeValuesInSearchResults]
        [Tokenize]
        [IncludeInDefaultSearch]
        public virtual string ThematicTag { get; set; }

        /* Adv. below */

        public virtual ContentArea ProductArea { get; set; }

        public virtual decimal Margin { get; set; } // Added for Adv + Find (.InRange)

        [Searchable]
        [IncludeValuesInSearchResults]
        [IncludeInDefaultSearch]
        public virtual string Brand { get; set; } // added for Adv. + Find ()

        public virtual string theTaxCategory { get; set; }


        Injected<IContentLoader> _loader;
        //Injected<ReferenceConverter> _refConv;
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);

            CatalogContentBase myParent = _loader.Service.Get<CatalogContentBase>(this.ParentLink);

            // Changed so the ServiceAPI works
            if (myParent.GetOriginalType() == typeof(NodeContent))
            {

                FashionNode fashionNode = (FashionNode)myParent;

                // sooo much easier now 
                this.TaxCategoryId = int.Parse(fashionNode.TaxCategories);
                this.theTaxCategory = CatalogTaxManager.GetTaxCategoryNameById(Int16.Parse(fashionNode.TaxCategories));

            }
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Helpers;

namespace CommerceTraining.Controllers
{
    public class MyPackageController : ContentController<MyPackage>
    {
        public ActionResult Index(MyPackage currentContent)
        {
            /* Implementation of action. You can create your own view model class that you pass to the view or
             * you can pass the page type for simpler templates */

            CheckOnStringDict(currentContent);


            return View(currentContent);
        }

        Injected<ICatalogSystem> catSys;
        private void CheckOnStringDict(MyPackage currentContent)
        {
            var stuff = currentContent.GetPropertyValue("StringDictDemo"); // nope

            CatalogEntryDto dto = catSys.Service.GetCatalogEntryDto(currentContent.Code);

            var row = dto.CatalogEntry.FirstOrDefault();

            MetaObject metaObject = MetaObject.Load(MetaDataContext.Instance, row.CatalogEntryId, row.MetaClassId);
            var x = metaObject["StringDictDemo"];


            System.Collections.Hashtable hash = ObjectHelper.GetMetaFieldValues(dto.CatalogEntry.FirstOrDefault());
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            
            if (hash.Contains("StringDictDemo"))
            {
                foreach (var item in hash.Keys)
                {
                    

                }
            }

            foreach (var item in hash)
            {
                
                
            }

        }
    }
}
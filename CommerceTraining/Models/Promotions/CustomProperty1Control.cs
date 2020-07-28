using System;
using System.Collections.Generic;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web.PropertyControls;
using EPiServer.Web.WebControls;

namespace CommerceTraining.Models.Promotions
{
    /// <summary>
    /// PropertyControl implementation used for rendering CustomProperty1 data.
    /// </summary>
    public class CustomProperty1Control : EPiServer.Web.PropertyControls.PropertyStringControl
    {
        /*
        Override CreateXXXControls to control the appearance of the property data in different rendering conditions.

        public override void CreateDefaultControls()        - Used when rendering the view mode.
        public override void CreateEditControls()           - Used when rendering the property in edit mode.
        public override void CreateOnPageEditControls()     - used when rendering the property for "On Page Edit".

        */

        /// <summary>
        /// Gets the CustomProperty1 instance for this IPropertyControl.
        /// </summary>
        /// <value>The property that is to be displayed or edited.</value>
        //public CustomProperty1 CustomProperty1
        //{
        //    get
        //    {
        //        return PropertyData as CustomProperty1;
        //    }
        //}
    }
}

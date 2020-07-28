using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace CommerceTraining.Models.Media
{
    [ContentType(DisplayName = "DefaultMedia1", GUID = "45c88fe1-407e-4636-be21-9a36ef33b82f", Description = "")]
    /*[MediaDescriptor(ExtensionString = "pdf,doc,docx")]*/
    public class DefaultMedia1 : MediaData
    {

        
        [Editable(true)]
        [Display(
            Name = "Description",
            Description = "Description field's description",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual String Description { get; set; }

    }
}
using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Blobs;

namespace CommerceTraining.Models.Media
{
    [ContentType(DisplayName = "ImageModel", GUID = "cb418237-9254-4192-b79d-1de0661151bf", Description = "")]
    [MediaDescriptor(ExtensionString = "jpg,jpeg,jpe,ico,gif,bmp,png")]
    public class ImageModel : ImageData
    {
        public virtual string imageDescription { get; set; }

    }
}
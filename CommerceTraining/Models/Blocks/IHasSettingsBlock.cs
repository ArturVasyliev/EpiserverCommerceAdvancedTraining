using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.Blocks
{
    public interface IHasSettingsBlock
    {
        SettingsBlock Settings { get; set; }
    }
}
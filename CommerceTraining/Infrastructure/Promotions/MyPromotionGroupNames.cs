using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CommerceTraining.Infrastructure.Promotions
{
    [GroupDefinitions]
    public static class MyPromotionGroupNames
    {
        // CMS stuff - Groups = Tabs
        [Display(GroupName = "GroupForNumbers", Order = 10, Description = "Here are numbers we have to set")]
        public const string GroupForNumbersTab = "GroupForNumbers";

        [Display(GroupName = "GroupForConditions", Order = 20, Description="Here are what we have")]
        public const string GroupForConditionsTab = "GroupForConditions";

        [Display(GroupName = "GroupForRewards", Order = 30, Description = "Here are what we need to decide")]
        public const string GroupForRewardsTab = "GroupForRewards";

        [Display(GroupName = "GroupForDisconts", Order = 0, Description = "Here are where we do dicounts")]
        public const string GroupForDiscountTab = "GroupForDiscounts";


    }
}
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.BusinessFoundation.Data.Meta;
using Mediachase.Commerce.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class BisFoundViewModel
    {
        public Boolean ClubCardExists { get; set; }
        public IEnumerable<EntityObject> ClubCards { get; set; }
        public ClubCard SelectedCard { get; set; }
        public IEnumerable<ContactEntity> ContactList { get; set; }
        public IEnumerable<MetaEnumItem> CardTypeList { get; set; }
        public bool IsNew { get; set; }
    }

    public class ClubCard
    {
        public int CardId { get; set; }
        public string TitleField { get; set; }
        public string CardOwnerName { get; set; }
        public string Email { get; set; }
        public int Balance { get; set; }
        public int CardType { get; set; }
        public Guid ContactId { get; set; }
    }
}
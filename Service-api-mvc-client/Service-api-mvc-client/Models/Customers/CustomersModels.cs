using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Integration.Client.Models.Customers
{
    class CustomersModels
    {
    }

    public class Contact
    {
        public Guid? PrimaryKeyId { get; set; }
        public Address[] Addresses { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RegistrationSource { get; set; }
    }

    public class Address
    {
        public Guid? AddressId { get; set; }
        public DateTime? Modified { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string RegionName { get; set; }
        public string RegionCode { get; set; }
        public string Email { get; set; }
        public bool ShippingDefault { get; set; }
        public bool BillingDefault { get; set; }
        public string DaytimePhoneNumber { get; set; }
        public string EveningPhoneNumber { get; set; }
        public string Organization { get; set; }
    }

    public class Organization
    {
        public Guid PrimaryKeyId { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public IEnumerable<Organization> ChildOrganizations { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
        public string OrganizationType { get; set; }
        public string OrgCustomerGroup { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Integration.Client.Models.Orders
{
    class OrderModels
    {
    }

    public class PropertyItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Discount
    {
        public int DiscountId { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountCode { get; set; }
        public string DiscountName { get; set; }
        public string DisplayMessage { get; set; }
        public decimal DiscountValue { get; set; }
    }

    public class Shipment
    {
        public Discount[] Discounts { get; set; }
        public int ShipmentId { get; set; }
        public Guid ShippingMethodId { get; set; }
        public string ShippingMethodName { get; set; }
        public decimal ShippingTax { get; set; }
        public string ShippingAddressId { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public decimal ShippingDiscountAmount { get; set; }
        public decimal ShippingSubTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public string Status { get; set; }
        public string PrevStatus { get; set; }
        public int? PickListId { get; set; }
        public decimal SubTotal { get; set; }
        public string WarehouseCode { get; set; }
        public LineItem[] LineItems { get; set; }
        public List<PropertyItem> Properties { get; set; }
    }

    public class OrderForm
    {
        public Shipment[] Shipments { get; set; }
        public LineItem[] LineItems { get; set; }
        public Discount[] Discounts { get; set; }
        public string ReturnComment { get; set; }
        public string ReturnType { get; set; }
        public string ReturnAuthCode { get; set; }
        public int OrderFormId { get; set; }
        public string Name { get; set; }
        public string BillingAddressId { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal HandlingTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string RmaNumber { get; set; }
        public decimal AuthorizedPaymentTotal { get; set; }
        public decimal CapturedPaymentTotal { get; set; }
        public List<PropertyItem> Properties { get; set; }
    }

    public class OrderNote
    {
        public int? OrderNoteId { get; set; }
        public DateTime Created { get; set; }
        public Guid CustomerId { get; set; }
        public string Detail { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int? LineItemId { get; set; }
    }

    public class LineItem
    {
        public int LineItemId { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public decimal PlacedPrice { get; set; }
        public decimal ExtendedPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReturnQuantity { get; set; }
        /*Values can be "Disabled" or "Enabled"*/
        public string InventoryTrackingStatus { get; set; }
        public bool IsInventoryAllocated { get; set; }
        public bool IsGift { get; set; }
    }

    public class OrderAddress
    {
        public int OrderGroupAddressId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string DaytimePhoneNumber { get; set; }
        public string EveningPhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Email { get; set; }
    }

    public class PurchaseOrder
    {
        public string AddressId { get; set; }
        public Guid AffiliateId { get; set; }
        public string BillingCurrency { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal HandlingTotal { get; set; }
        public Guid InstanceId { get; set; }
        public string MarketId { get; set; }
        public string Name { get; set; }
        public string OrderNumber { get; set; }
        public OrderAddress[] OrderAddresses { get; set; }
        public OrderForm[] OrderForms { get; set; }
        public int OrderGroupId { get; set; }
        public OrderNote[] OrderNotes { get; set; }
        public string Owner { get; set; }
        public string OwnerOrg { get; set; }
        public string ProviderId { get; set; }
        public decimal ShippingTotal { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal Total { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public List<PropertyItem> Properties { get; set; }
    }

}

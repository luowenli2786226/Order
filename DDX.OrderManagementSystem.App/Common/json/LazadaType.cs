using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App.Lazada
{
    public class AddressShipping
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string CustomerEmail { get; set; }
        public string City { get; set; }
        public string Ward { get; set; }
        public string Region { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }

    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerFirstName { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string Remarks { get; set; }
        public string DeliveryInfo { get; set; }
        public string Price { get; set; }
        public string GiftOption { get; set; }
        public string GiftMessage { get; set; }
        public string VoucherCode { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string NationalRegistrationNumber { get; set; }
        public string PromisedShippingTime { get; set; }
        public string ExtraAttributes { get; set; }
        public AddressShipping AddressShipping { get; set; }
    }

    public class Orders
    {
        public Order Order { get; set; }
    }
    public class Body
    {
        public Orders Orders { get; set; }
    }

    public class SuccessResponse
    {
        public Body Body { get; set; }
    }

    public class LazadaOrderList
    {
        //public string message { get; set; }
        //public string code { get; set; }
        //public List<LazadaOrder> Orders { get; set; }
        public SuccessResponse SuccessResponse { get; set; }
    }
}
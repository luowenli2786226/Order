using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class BLSCustoms
    {
        public string Sku { get; set; }
        public string ChineseContentDescription { get; set; }
        public string ItemContent { get; set; }
        public string ItemCount { get; set; }
        public string Value { get; set; }
        public string Currency { get; set; }
        public string Weight { get; set; }
        public string SkuInInvoice { get; set; }
    }

    public class BLSRootObject
    {
        public string ContractId { get; set; }
        public string OrderNumber { get; set; }
        public string RecipientName { get; set; }
        public string RecipientStreet { get; set; }
        public string RecipientHouseNumber { get; set; }
        public string RecipientBusnumber { get; set; }
        public string RecipientZipCode { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientState { get; set; }
        public string RecipientCountry { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderSequence { get; set; }
     
        public List<BLSCustoms> Customs { get; set; }
    }
}
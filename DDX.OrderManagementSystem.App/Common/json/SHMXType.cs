using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App.Common.json
{
    public class IdentifyReturnType
    {
        public bool ack { get; set; }
        public string customer_id { get; set; }
        public string customer_userid { get; set; }
    }
    public class OrderReturnType
    {
        public bool ack { get; set; }
        public string message { get; set; }
        public string reference_number { get; set; }
        public string tracking_number { get; set; }
        public string order_id { get; set; }
    }
    public class OrderRquestParam
    {
        public string buyerid { get; set; }
        public string consignee_address { get; set; }
        public string consignee_city { get; set; }
        public string consignee_mobile { get; set; }
        public string consignee_name { get; set; }
        public string trade_type { get; set; }
        public string consignee_postcode { get; set; }
        public string consignee_state { get; set; }
        public string consignee_telephone { get; set; }
        public string country { get; set; }
        public string customer_id { get; set; }
        public string customer_userid { get; set; }
        public List<orderInvoiceParam> orderInvoiceParam { get; set; }
        public string order_customerinvoicecode { get; set; }
        public string product_id { get; set; }
        public string weight { get; set; }
    }
    public class orderInvoiceParam
    {
        public string invoice_amount { get; set; }
        public string invoice_pcs { get; set; }
        public string invoice_title { get; set; }
        public string invoice_weight { get; set; }
        public string item_id { get; set; }
        public string item_transactionid { get; set; }
        public string sku { get; set; }
    }
}
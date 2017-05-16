using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App.Common
{
    public class ShippingDetail
    {
        public string phone_number { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string street_address2 { get; set; }
        public string street_address1 { get; set; }
        public string zipcode { get; set; }
    }

    public class OrderInfo
    {
        public string sku { get; set; }
        public string buyer_id { get; set; }
        public string last_updated { get; set; }
        public string product_id { get; set; }
        public string order_time { get; set; }
        public string quantity { get; set; }
        public string color { get; set; }
        public string price { get; set; }
        public string shipping_cost { get; set; }
        public string shipping { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
        public string order_id { get; set; }
        public string state { get; set; }
        public string cost { get; set; }
        public string variant_id { get; set; }
        public string order_total { get; set; }
        public string days_to_fulfill { get; set; }
        public string product_image_url { get; set; }
        public string product_name { get; set; }
        public string transaction_id { get; set; }
        public string size { get; set; }
    }

    public class WishOrder
    {
        public OrderInfo Order { get; set; }
    }

    public class Paging
    {
        public string next { get; set; }
    }

    public class WishOrderList
    {
        public string message { get; set; }
        public string code { get; set; }
        public List<WishOrder> data { get; set; }
        public Paging paging { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App.Common
{
    public class BellaBuyProducts
    {
        public string product_name { get; set; }
        public string sku { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public string product_price { get; set; }
        public string product_quantity { get; set; }
        public string product_img { get; set; }
    }

    public class BellaBuyData
    {
        public string order_id { get; set; }
        public string name { get; set; }
        public string zipcode { get; set; }
        public string telphone { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string mobile { get; set; }
        public string order_total { get; set; }
        public string order_time { get; set; }
        public string status { get; set; }
        public List<BellaBuyProducts> products { get; set; }
    }

    public class BellaBuyRootObject
    {
        public string code { get; set; }
        public string message { get; set; }
        public int totalpage { get; set; }
        public List<BellaBuyData> data { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class SUBItem
    {
        public string proName { get; set; }
        public string qnt { get; set; }
        public string sku { get; set; }
    }

    public class SUBOrderRootObject
    {
        public string hasLabel { get; set; }
        public string addr1 { get; set; }
        public string addr2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string decValue { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public List<SUBItem> item { get; set; }
        public string memo { get; set; }
        public string phone { get; set; }
        public string saleNo { get; set; }
        public string state { get; set; }
        public string trNum { get; set; }
        public string typeCode { get; set; }
        public string weight { get; set; }
        public string zip { get; set; }
    }




    public class ResultItem
    {
        public string id { get; set; }
        public string proName { get; set; }
        public string qnt { get; set; }
        public string sku { get; set; }
    }

    public class Obj
    {
        public string addr1 { get; set; }
        public string addr2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string crTime { get; set; }
        public string decValue { get; set; }
        public string email { get; set; }
        public string fee { get; set; }
        public string fullName { get; set; }
        public List<ResultItem> item { get; set; }
        public string lastTime { get; set; }
        public string memo { get; set; }
        public string pTime { get; set; }
        public string payTime { get; set; }
        public string phone { get; set; }
        public string saleNo { get; set; }
        public string serNum { get; set; }
        public string state { get; set; }
        public string status { get; set; }
        public string trNum { get; set; }
        public string typeCode { get; set; }
        public string weight { get; set; }
        public string zip { get; set; }
        public string zone { get; set; }
    }

    public class Result
    {
        public string statusCode { get; set; }
        public string label { get; set; }
        public Obj obj { get; set; }
        public string msg { get; set; }
    }

    public class SUBOrderResult
    {
        public Result result { get; set; }
        public string respStatus { get; set; }
        public string msg { get; set; }
    }

}
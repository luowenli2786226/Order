using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class MonthlySalesModel
    {
        public string Account { get; set; }
        public string AccountUrl { get; set; }
        public string Rate { get; set; }
        public int Monthly { get; set; }
        public string Year { get; set; }
        public string CurrencyCode { get; set; }
        public double UnitPrice { get; set; }
        public int OrderCount { get; set; }
        public double Amount { get; set; }
    }

    public class MonthlySalesCountModel
    {
        public string Account { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountUrl { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string H { get; set; }
        public string I { get; set; }
        public string J { get; set; }
        public string K { get; set; }
        public string L { get; set; }


    }
}
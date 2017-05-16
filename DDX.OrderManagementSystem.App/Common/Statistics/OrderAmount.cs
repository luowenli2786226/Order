using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class OrderAmountModel
    {
        public string Account { get; set; }

        public string Platform { get; set; }
        /// <summary>
        /// 总订单数
        /// </summary>
        public int OCount { get; set; }

        public double Profit { get; set; }
        public double Loss { get; set; }
        public double SUMProfit { get; set; }
        public double SumAmount { get; set; }
        public double ProfitRate { get; set; }
        public string Area { get; set; }

        public string CurrencyCode { get; set; }

    }
}
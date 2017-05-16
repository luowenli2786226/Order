using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    /// <summary>
    /// 比例分析
    /// </summary>
    public class LeveDays
    {
        public string Platform { get; set; }

        /// <summary>
        /// 比例
        /// </summary>
        public decimal OCount { get; set; }

        public String Account { get; set; }

        public int TotalCount { get; set; }

        public decimal Rate { get; set; }

        public string Area { get; set; }
    }

    public class UnSendSKUModel
    {
        public string SKU { get; set; }

        public int Qty { get; set; }

        public DateTime? MinDate { get; set; }

        public int QQQ { get; set; }
    }
}
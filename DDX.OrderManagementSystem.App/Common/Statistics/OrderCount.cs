using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class OrderCount
    {
        public string Account { get; set; }

        public string Platform { get; set; }
        /// <summary>
        /// 总订单数
        /// </summary>
        public int OCount { get; set; }

        /// <summary>
        /// 货币
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public double TotalAmount { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public double AmountUSD { get; set; }
        /// <summary>
        /// 配货订单
        /// </summary>
        public int PCount { get; set; }

        /// <summary>
        /// 已发货订单
        /// </summary>
        public int SCount { get; set; }

        /// <summary>
        /// 缺货订单
        /// </summary>
        public int QCount { get; set; }

        /// <summary>
        /// 缺货订单
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string ManageBy { get; set; }

        /// <summary>
        /// 缺货订单
        /// </summary>
        public string AccountUrl { get; set; }
        


    }
}
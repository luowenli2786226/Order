﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class QueCount
    {
        public string SKU { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int Qty { get; set; }
        /// <summary>
        /// 总数
        /// </summary>
        public int SQty { get; set; }

        /// <summary>
        /// 总订单数
        /// </summary>
        public int OrderQty { get; set; }

        public string Standard { get; set; }

        public string Caigou { get; set; }

        public DateTime MinDate { get; set; }
        public DateTime LastDate { get; set; }

        public string Field1 { get; set; }

        public int BuyQty { get; set; }

        public int UnPeiQty { get; set; }

        public int NowQty { get; set; }

        public int NeedQty { get; set; }

        public int SNeedQty { get; set; }

        public string Pic { get; set; }

        public string Memo { get; set; }


    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    /// <summary>
    /// 比例分析
    /// </summary>
    public class LeveData
    {
        public string Platform { get; set; }

        /// <summary>
        /// 比例
        /// </summary>
        public decimal OCount { get; set; }

        public int Account { get; set; }

        public int TotalCount { get; set; }
    }
}
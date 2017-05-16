using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class ProductData
    {
        public string SKU { get; set; }

        public int Qty { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string Category { get; set; }

        public int OQty { get; set; }

        public string PicUrl { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public string Standard { get; set; }

        public string OAmount { get; set; }

        public double Rate { get; set; }

        public string Price { get; set; }
        public string Weight { get; set; }
        
        public double TotalPrice { get; set; }
        public double TotalWeight { get; set; }
        
        public string Remark { get; set; }
        //库存
        public string SourceQty { get; set; }

        //库存总金额
        public double SourceAmount { get; set; }

        //采购（预）【采购未到数-缺货数】
        public string PurchaseplanQty { get; set; }
        //已处理  缺货否
        public double HandingQty { get; set; }
    }
}
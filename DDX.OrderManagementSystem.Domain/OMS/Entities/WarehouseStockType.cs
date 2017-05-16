//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseStockType
    /// 仓库库存表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0  创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name></name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class WarehouseStockType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }


        /// <summary>
        /// 主键
        /// </summary>
        public virtual string InNo { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public virtual int WId { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public virtual String Warehouse { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public virtual String Pic { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public virtual int PId { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public virtual double Price { get; set; }
        /// <summary>
        /// 总
        /// </summary>
        public virtual double TotalPrice { get; set; }

        /// <summary>
        /// 商品SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public virtual String Title { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Qty { get; set; }

        /// <summary>
        /// 在仓库
        /// </summary>
        public virtual int UnPeiQty { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTime UpdateOn { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public virtual String UpdateBy { get; set; }

        /// <summary>
        /// 保有量
        /// </summary>
        public virtual double Parc { get; set; }

        /// <summary>
        /// 保有量计算公式
        /// </summary>
        public virtual String Formula { get; set; }

        /// <summary>
        /// SKU相关所有仓库数量
        /// </summary>
        public virtual int AllQty { get; set; }

        /// <summary>
        /// SKU库位
        /// </summary>
        public virtual String Location { get; set; }
    }
}

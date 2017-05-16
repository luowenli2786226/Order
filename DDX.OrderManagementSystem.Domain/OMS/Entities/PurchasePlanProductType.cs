//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PurchasePlanProductType
    /// 采购计划产品
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
    public class PurchasePlanProductType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 计划ID
        /// </summary>
        public virtual int PID { get; set; }

        /// <summary>
        /// 产品SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// 产品数量
        /// </summary>
        public virtual int Qty { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public virtual String Standard { get; set; }

    }
}

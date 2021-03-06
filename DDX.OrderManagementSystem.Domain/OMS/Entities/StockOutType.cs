﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// StockOutType
    /// 出库记录表
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
    public class StockOutType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public virtual String OrderNo { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public virtual int PId { get; set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        public virtual String OutType { get; set; }



        /// <summary>
        /// 主键
        /// </summary>
        public virtual string InNo { get; set; }
        /// <summary>
        /// Memo
        /// </summary>
        public virtual String Memo { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public virtual Decimal Price { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public virtual int IsAudit { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Qty { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Enabled { get; set; }

        /// <summary>
        /// 原有库存
        /// </summary>
        public virtual int SourceQty { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public virtual int WId { get; set; }

        /// 仓库ID
        /// </summary>
        public virtual string WName { get; set; }

        /// <summary>
        /// 出库人
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// 出库时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

    }
}

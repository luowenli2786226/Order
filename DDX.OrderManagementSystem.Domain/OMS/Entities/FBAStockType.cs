//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// FBAStockType
    /// FBAStock
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
    public class FBAStockType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public virtual  string Account { get; set; }
        /// <summary>
        /// SKU
        /// </summary>
        public virtual String SKU { get; set; }
        /// <summary>
        /// 商品状况
        /// </summary>
        public virtual string Condition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string FNSKU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string ASIN { get; set; }

        /// <summary>
        /// Pid
        /// </summary>
        public virtual int Pid { get; set; }

        /// <summary>
        /// Pic
        /// </summary>
        public virtual String Pic { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public virtual String Title { get; set; }

        /// <summary>
        /// Qty
        /// </summary>
        public virtual int Qty { get; set; }

        /// <summary>
        /// TotalQty
        /// </summary>
        public virtual  int TotalQty { get; set; }
        /// <summary>
        /// 转运中的数量
        /// </summary>
        public virtual decimal TransferQty { get; set; }

        /// <summary>
        /// CreateOn
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// UndateOn
        /// </summary>
        public virtual DateTime UpdateOn { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public virtual String Remark { get; set; }
        /// <summary>
        /// 配送渠道
        /// </summary>
        public virtual string FulfillmentChannel { get; set; }

    }
}

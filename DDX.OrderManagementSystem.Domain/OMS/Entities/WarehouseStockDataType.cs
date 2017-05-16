//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , HanRuiOMS TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseStockDataType
    /// 库存明细表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 XiDOng 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDOng</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class WarehouseStockDataType
    {
        /// <summary>
        /// 标识
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 入库ID
        /// </summary>
        public virtual int InId { get; set; }

        /// <summary>
        /// 入库序列号
        /// </summary>
        public virtual String InNo { get; set; }

        /// <summary>
        /// 仓库状态
        /// </summary>
        public virtual int State { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public virtual int PId { get; set; }

        /// <summary>
        /// 产品SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// 产品
        /// </summary>
        public virtual String PName { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public virtual String MainSKU { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public virtual int WId { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public virtual String WName { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public virtual int Qty { get; set; }

        /// <summary>
        /// 现有数量
        /// </summary>
        public virtual int NowQty { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public virtual Decimal Freight { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public virtual Decimal Amount { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public virtual DateTime? ProductionOn { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public virtual DateTime? ExpirationOn { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime? CreateOn { get; set; }

        /// <summary>
        /// 金额小计
        /// </summary>
        public virtual double Total { get; set; }
        /// <summary>
        /// 单价背景颜色
        /// </summary>
        public virtual string Style { get; set; }

    }
}

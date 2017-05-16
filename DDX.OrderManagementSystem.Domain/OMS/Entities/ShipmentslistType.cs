//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ShipmentslistType
    /// 发货清单表
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
    public class ShipmentslistType
    {
        /// <summary>
        /// 出货清单Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 采购合同号码
        /// </summary>
        public virtual String ContractPNo { get; set; }

        /// <summary>
        /// 外销合同号码
        /// </summary>
        public virtual String ContractWNo { get; set; }

        /// <summary>
        /// 审批进程
        /// </summary>
        public virtual string IsExa { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public virtual String ExaTime { get; set; }

        /// <summary>
        /// 第一次确认时间
        /// </summary>
        public virtual String OverTime1 { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public virtual String AppliBy { get; set; }

        /// <summary>
        /// 审批人
        /// </summary>
        public virtual String AgreeBy { get; set; }

        /// <summary>
        /// 第一确认人
        /// </summary>
        public virtual String OkBy1 { get; set; }

        /// <summary>
        /// 第二次确认时间
        /// </summary>
        public virtual String OverTime2 { get; set; }

        /// <summary>
        /// 第二次确认人
        /// </summary>
        public virtual String OkBy2 { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public virtual DateTime AppliTime { get; set; }
        /// <summary>
        /// 移除的海外仓库名称
        /// </summary>
        public virtual string WareHouse { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Remark { get; set; }

        /// 优胜价格(总)
        /// </summary>
        public virtual double YouShengtotal { get; set; }

        /// 工厂总价(总)
        /// </summary>
        public virtual double PriceFactorytotal { get; set; }

        /// 毛重(总)
        /// </summary>
        public virtual double WeightGrosstotal { get; set; }

        /// 净重(总)
        /// </summary>
        public virtual double WeightNettotal { get; set; }
        /// 体积(总)
        /// </summary>
        public virtual double TotalVolume { get; set; }

        /// 数量(总)
        /// </summary>
        public virtual double Qty { get; set; }
        /// <summary>
        /// 客人含佣金总价
        /// </summary>
        public virtual double TotalPrice { get; set; }

    }
}

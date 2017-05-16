//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// FixedRateType
    /// 固定汇率
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
    public class FixedRateType
    {
        /// <summary>
        /// 主键表
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 货币
        /// </summary>
        public virtual String CurrencyName { get; set; }

        /// <summary>
        /// 货币
        /// </summary>
        public virtual String CurrencyCode { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public virtual decimal CurrencyValue { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public virtual int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public virtual int Month { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public virtual String CreateBy { get; set; }
        
        /// <summary>
        /// 操作日期
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

    }
}

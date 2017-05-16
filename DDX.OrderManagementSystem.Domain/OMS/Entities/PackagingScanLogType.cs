//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// PackagingScanLogType
    /// 包装扫描日志表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0  创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// 版本：1.1  增加订单号。
    /// 
    /// 版本1.1
    /// 
    /// <author>
    /// <name></name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class PackagingScanLogType
    {
        /// <summary>
        /// 主键标识
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 包装类型
        /// </summary>
        public virtual String PackageType { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public virtual String Operator { get; set; }

        /// <summary>
        /// 操作日期
        /// </summary>
        public virtual DateTime OperationOn { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public virtual String OrderNo { get; set; }

    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// OutInDetailsType
    /// 出入库记录表
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
    public class OutInDetailsType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual String Id { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public virtual String OutInType { get; set; }

        /// <summary>
        /// 相关订单
        /// </summary>
        public virtual String OrderNo { get; set; }

        /// <summary>
        /// 仓库名
        /// </summary>
        public virtual String WName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Qty { get; set; }

        /// <summary>
        /// 原有库存
        /// </summary>
        public virtual int SourceQty { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// 出入库类型
        /// </summary>
        public virtual String Type { get; set; }
    }
}
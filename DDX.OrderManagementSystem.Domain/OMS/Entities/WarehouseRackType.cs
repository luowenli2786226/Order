//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseRackType
    /// 仓库货架表
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
    public class WarehouseRackType
    {
        /// <summary>
        /// 货架Id
        /// </summary>
        public virtual int RackId { get; set; }

        /// <summary>
        /// 排位Id
        /// </summary>
        public virtual int LineId { get; set; }

        /// <summary>
        /// 排位编码
        /// </summary>
        public virtual String LineCode { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public virtual String RackCode { get; set; }

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

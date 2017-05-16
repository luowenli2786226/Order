//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseAreaType
    /// 仓库区域表
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
    public class WarehouseAreaType
    {
        /// <summary>
        /// 区域Id
        /// </summary>
        public virtual int AreaId { get; set; }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public virtual int WId { get; set; }

        /// <summary>
        /// 仓库名
        /// </summary>
        public virtual String WName { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public virtual String AreaCode { get; set; }

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

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseLocationType
    /// 库位表
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
    public class WarehouseLocationType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Wid
        /// </summary>
        public virtual int Wid { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public virtual String WName { get; set; }

        /// <summary>
        /// 库位名称
        /// </summary>
        public virtual String Code { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public virtual int SortCode { get; set; }

        /// <summary>
        /// 上级
        /// </summary>
        public virtual int ParentId { get; set; }
        /// <summary>
        /// RackCode
        /// </summary>
        public virtual string RackCode { get; set; }

    }
}

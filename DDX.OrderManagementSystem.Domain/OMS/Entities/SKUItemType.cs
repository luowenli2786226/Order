//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// SKUItemType
    /// SKUItem
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 XiDond 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDond</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class SKUItemType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// SKUEx
        /// </summary>
        public virtual String SKUEx { get; set; }

        /// <summary>
        /// Account
        /// </summary>
        public virtual String Account { get; set; }

    }
}

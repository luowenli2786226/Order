//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// WarehouseRackType
    /// 仓库州对应表
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

    public class WarehouseProInfoType
    {
       /// <summary>
       /// 主键
       /// </summary>
       public virtual int Id { get; set; }
       /// <summary>
       /// 仓库
       /// </summary>
       public virtual string WareHouse { get; set; }
       /// <summary>
       /// 州
       /// </summary>
       public virtual string Province { get; set; }
       /// <summary>
       /// 排序
       /// </summary>
       public virtual int Orderby { get; set; }
    }
}

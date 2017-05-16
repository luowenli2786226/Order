//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseLocationTypeMap
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
    public class WarehouseLocationTypeMap : ClassMap<WarehouseLocationType> 
    {
        public WarehouseLocationTypeMap()
        {
            Table("WarehouseLocation");
            Id(x => x.Id);
            Map(x => x.Wid);
            Map(x => x.WName).Length(100);
            Map(x => x.Code).Length(50);
            Map(x => x.Remark).Length(300);
            Map(x => x.SortCode);
            Map(x => x.ParentId);
        }
    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseRackTypeMap
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
    public class WarehouseRackTypeMap : ClassMap<WarehouseRackType> 
    {
        public WarehouseRackTypeMap()
        {
            Table("WarehouseRack");
            Id(x => x.RackId);
            Map(x => x.LineId);
            Map(x => x.LineCode).Length(20);
            Map(x => x.RackCode).Length(20);
            Map(x => x.CreateBy).Length(20);
            Map(x => x.CreateOn);

        }
    }
}

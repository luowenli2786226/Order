//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseLineTypeMap
    /// 仓库排位表
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
    public class WarehouseLineTypeMap : ClassMap<WarehouseLineType> 
    {
        public WarehouseLineTypeMap()
        {
            Table("WarehouseLine");
            Id(x => x.LineId);
            Map(x => x.AreaId);
            Map(x => x.AreaCode).Length(20);
            Map(x => x.LineCode).Length(20);
            Map(x => x.CreateBy).Length(20);
            Map(x => x.CreateOn);

        }
    }
}

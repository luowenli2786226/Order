//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseAreaTypeMap
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
    public class WarehouseAreaTypeMap : ClassMap<WarehouseAreaType> 
    {
        public WarehouseAreaTypeMap()
        {
            Table("WarehouseArea");
            Id(x => x.AreaId);
            Map(x => x.WId);
            Map(x => x.WName).Length(50);
            Map(x => x.AreaCode).Length(20);
            Map(x => x.CreateBy).Length(20);
            Map(x => x.CreateOn);

        }
    }
}

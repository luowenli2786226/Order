//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ShipmentlistTypeMap
    /// 发货清单表
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
    public class ShipmentslistTypeMap : ClassMap<ShipmentslistType>
    {
        public ShipmentslistTypeMap()
        {
            Table("Shipmentslist");
            Id(x => x.Id);
            Map(x => x.ContractPNo).Length(600);
            Map(x => x.ContractWNo).Length(600);
            Map(x => x.IsExa).Length(50);
            Map(x => x.ExaTime).Length(600);
            Map(x => x.OverTime1).Length(600);
            Map(x => x.AppliBy).Length(600);
            Map(x => x.AgreeBy).Length(600);
            Map(x => x.OkBy1).Length(600);
            Map(x => x.OverTime2).Length(600);
            Map(x => x.OkBy2).Length(600);
            Map(x => x.AppliTime).Length(600);
            Map(x => x.WareHouse).Length(50);
            Map(x => x.Remark).Length(500);

        }
    }
}

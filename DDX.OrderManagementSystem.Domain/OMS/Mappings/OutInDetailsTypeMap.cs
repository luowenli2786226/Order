//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// OutInDetailsMap
    /// 出入库记录表
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
    public class OutInDetailsTypeMap : ClassMap<OutInDetailsType> 
    {
        public OutInDetailsTypeMap()
        {
            Table("OutInDetails");
            Id(x => x.Id).Length(100);
            Map(x => x.CreateOn);
            Map(x => x.OutInType).Length(100);
            Map(x => x.OrderNo).Length(100);
            Map(x => x.WName).Length(100);
            Map(x => x.Qty);
            Map(x => x.SourceQty);
            Map(x => x.CreateBy).Length(100);
            Map(x => x.Type).Length(100);
        }
    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PurchasePlanProductTypeMap
    /// 采购计划产品
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
    public class PurchasePlanProductTypeMap : ClassMap<PurchasePlanProductType> 
    {
        public PurchasePlanProductTypeMap()
        {
            Table("PurchasePlanProducts");
            Id(x => x.Id);
            Map(x => x.PID);
            Map(x => x.SKU).Length(200);
            Map(x => x.Qty);
            Map(x => x.Standard).Length(400);
        }
    }
}

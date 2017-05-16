//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// SKUItemTypeMap
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
    public class SKUItemTypeMap : ClassMap<SKUItemType> 
    {
        public SKUItemTypeMap()
        {
            Table("SKUItem");
            Id(x => x.Id);
            Map(x => x.SKU).Length(200);
            Map(x => x.SKUEx).Length(200);
            Map(x => x.Account).Length(200);
        }
    }
}

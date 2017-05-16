//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// FBAStockTypeMap
    /// FBAStock
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
    public class FBAStockTypeMap : ClassMap<FBAStockType> 
    {
        public FBAStockTypeMap()
        {
            Table("FBAStock");
            Id(x => x.Id);
            Map(x => x.Account);
            Map(x => x.SKU).Length(100);
            Map(x => x.Condition);
            Map(x => x.FNSKU);
            Map(x => x.ASIN);
            Map(x => x.Pid);
            Map(x => x.Pic).Length(800);
            Map(x => x.Title).Length(400);
            Map(x => x.Qty);
            Map(x => x.TotalQty);
            Map(x => x.TransferQty);
            Map(x => x.CreateOn);
            Map(x => x.CreateBy).Length(100);
            Map(x => x.UpdateOn);
            Map(x => x.Remark).Length(400);
            Map(x => x.FulfillmentChannel);
        }
    }
}

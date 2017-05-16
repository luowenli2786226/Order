//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , HanRuiOMS TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseStockDataTypeMap
    /// 库存明细表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 XiDOng 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDOng</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class WarehouseStockDataTypeMap : ClassMap<WarehouseStockDataType> 
    {
        public WarehouseStockDataTypeMap()
        {
            Table("WarehouseStockData");
            Id(x => x.Id);
            Map(x => x.InId);
            Map(x => x.InNo).Length(200);
            Map(x => x.State);
            Map(x => x.PId);
            Map(x => x.SKU).Length(200);
            Map(x => x.PName).Length(800);
            Map(x => x.MainSKU).Length(200);
            Map(x => x.WId);
            Map(x => x.WName).Length(200);
            Map(x => x.Qty);
            Map(x => x.NowQty);
            Map(x => x.Remark).Length(800);
            Map(x => x.Freight).Length(18);
            Map(x => x.Amount).Length(18);
            Map(x => x.ProductionOn);
            Map(x => x.ExpirationOn);
            Map(x => x.CreateBy).Length(200);
            Map(x => x.CreateOn);
        }
    }
}

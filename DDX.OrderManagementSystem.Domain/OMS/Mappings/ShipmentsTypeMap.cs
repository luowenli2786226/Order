//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ShipmentsTypeMap
    /// 平台账户表
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
    public class ShipmentsTypeMap : ClassMap<ShipmentsType>
    {
        public ShipmentsTypeMap()
        {
            Table("Shipments");
            Id(x => x.Id);
            Map(x => x.DescribeEn).Length(500);
            Map(x => x.DescribeCn).Length(500);
            Map(x => x.Sku).Length(50);
            Map(x => x.PurchaseNo).Length(50);
            Map(x => x.ExportNo).Length(50);
            Map(x => x.PriceUMax);
            Map(x => x.PriceFactory);
            Map(x => x.Unit).Length(20);
            Map(x => x.PackageNo);
            Map(x => x.Ctn);
            Map(x => x.IsCustoms);
            Map(x => x.WeightGross);
            Map(x => x.WeightNet);
            Map(x => x.TotalVolume);
            Map(x => x.Qty);
            Map(x => x.FirstQty);
            Map(x => x.TotalPrice);
            Map(x => x.TaxRate);
            Map(x => x.Ratio);
            Map(x => x.CreateBy).Length(50);
            Map(x => x.CreatePlanBy).Length(50);
            Map(x => x.CreatetTrackBy).Length(50);
            Map(x => x.Paymethod).Length(50);
            Map(x => x.HeadloadCharges);
            Map(x => x.YouShengPrice);
            Map(x => x.CreateOn).Length(600);
            Map(x => x.ShipmentslistId);
            Map(x => x.UpdateBy).Length(50);
            Map(x => x.UpdateTime);
        }
    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PurchasePlanTypeMap
    /// 采购计划表
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
    public class PurchasePlanTypeMap : ClassMap<PurchasePlanType>
    {
        public PurchasePlanTypeMap()
        {
            Table("PurchasePlan");
            Id(x => x.Id);
            Map(x => x.PlanNo).Length(50);
            Map(x => x.SKU).Length(50);
            Map(x => x.Price);
            Map(x => x.Qty);
            Map(x => x.DaoQty);
            Map(x => x.Freight);
            Map(x => x.ProductName).Length(200);
            Map(x => x.ProductUrl).Length(300);
            Map(x => x.PicUrl).Length(300);
            Map(x => x.Suppliers).Length(50);
            Map(x => x.SId);
            Map(x => x.LogisticsMode).Length(50);
            Map(x => x.TrackCode).Length(50);
            Map(x => x.PostStatus).Length(50);
            Map(x => x.Status).Length(20);
            Map(x => x.ProcurementModel).Length(20);
            Map(x => x.BuyBy).Length(50);
            Map(x => x.CreateBy).Length(50);
            Map(x => x.OrderNo);
            Map(x => x.CreateOn);
            Map(x => x.Area);
            Map(x => x.FromTo);
            Map(x => x.BuyOn);
            Map(x => x.SendOn);
            Map(x => x.UnitTariff);
            Map(x => x.ReceiveOn);
            Map(x => x.UnitFristPrice);
            Map(x => x.Profit);
            Map(x => x.ExpectReceiveOn);
            Map(x => x.IsExamine);
            Map(x => x.ExamineId);
            Map(x => x.Memo).Length(800);
            Map(x => x.TuiFreight);
            Map(x => x.TuiPrice);
            Map(x => x.SettlementType).Length(800);
            Map(x => x.IsBei);
            Map(x => x.ErrorType).Length(800);
            Map(x => x.HandleType).Length(800);
            Map(x => x.ErrorRemark).Length(800);
            Map(x => x.IsFrist);
            Map(x => x.IsTuiFreight);
            Map(x => x.IsTuiPrice);
            Map(x => x.MinDate);
            Map(x => x.MinValiDate);
            Map(x => x.WId);
        }
    }
}

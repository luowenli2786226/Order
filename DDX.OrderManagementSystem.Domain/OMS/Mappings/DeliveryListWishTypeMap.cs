//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DeliveryListWishTypeMap
    /// Wish发货清单
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
    public class DeliveryListWishTypeMap : ClassMap<DeliveryListWishType>
    {
        public DeliveryListWishTypeMap()
        {
            Table("DeliveryListWish");
            Id(x => x.Id);
            Map(x => x.SendOn);
            Map(x => x.SendBy).Length(50);
            Map(x => x.IsReissue).Length(50);
            Map(x => x.SKU).Length(50);
            Map(x => x.Qty);
            Map(x => x.Weight);
            Map(x => x.Platform).Length(50);
            Map(x => x.Account).Length(50);
            Map(x => x.SalesSite).Length(50);
            Map(x => x.Warehouse).Length(50);
            Map(x => x.PackageNo).Length(50);
            Map(x => x.Logistics).Length(50);
            Map(x => x.TotalWeight);
            Map(x => x.TotalFreight);
            Map(x => x.TrackCode).Length(50);
            Map(x => x.OrderNo).Length(100);
            Map(x => x.ItemId).Length(50);
            Map(x => x.ItemTitle).Length(50);
            Map(x => x.BuyerId).Length(100);
            Map(x => x.BuyerName).Length(100);
            Map(x => x.Country).Length(50);
            Map(x => x.Address1).Length(500);
            Map(x => x.Address2).Length(500);
            Map(x => x.City).Length(100);
            Map(x => x.Province).Length(100);
            Map(x => x.PostCode).Length(100);
            Map(x => x.Phone).Length(50);
            Map(x => x.Telphone).Length(50);
            Map(x => x.FullAddress).Length(500);
            Map(x => x.PayOn);
            Map(x => x.SaleOn);
            Map(x => x.CollectionPayPal).Length(50);
            Map(x => x.PaymentPayPal).Length(50);
            Map(x => x.Merchandiser).Length(50);
            Map(x => x.ProductDeveloper).Length(50);
            Map(x => x.InquiryClerk).Length(50);
            Map(x => x.PurchasingAgent).Length(50);
            Map(x => x.CurrencyCode).Length(50);
            Map(x => x.TotalPrice);
            Map(x => x.TotalPriceRMB);
            Map(x => x.Price);
            Map(x => x.PriceRMB);
            Map(x => x.Cost);
            Map(x => x.PlatformDealCurrency).Length(50);
            Map(x => x.PlatformDealFee);
            Map(x => x.PlatformDealFeeRMB);
            Map(x => x.PayPalFeeCurrency).Length(50);
            Map(x => x.PayPalFee);
            Map(x => x.PayPalFeeRMB);
            Map(x => x.PlatformFee);
            Map(x => x.FirstLegLogistics).Length(50);
            Map(x => x.FirstLegFreight);
            Map(x => x.FirstLegDeclarationFee);
            Map(x => x.PackingMaterial).Length(50);
            Map(x => x.PackagingFee);
            Map(x => x.Freight);
            Map(x => x.Profit);
            Map(x => x.ProfitRate);
            Map(x => x.Area).Length(50);
            Map(x => x.IsFreight);
            Map(x => x.ActualFreight);
            Map(x => x.IsFan);
            Map(x => x.FanAmount);
            Map(x => x.Operator);
            Map(x => x.OperateTime);
        }
    }
}
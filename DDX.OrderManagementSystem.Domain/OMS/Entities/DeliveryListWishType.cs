//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DeliveryListWishType
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
    public class DeliveryListWishType
    {
        /// <summary>
        /// DeliveryListWishId
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public virtual DateTime SendOn { get; set; }

        /// <summary>
        /// 执行发货人
        /// </summary>
        public virtual String SendBy { get; set; }

        /// <summary>
        /// 是否补发货
        /// </summary>
        public virtual String IsReissue { get; set; }

        /// <summary>
        /// SKU
        /// </summary>
        public virtual String SKU { get; set; }

        /// <summary>
        /// 发货数量
        /// </summary>
        public virtual double Qty { get; set; }

        /// <summary>
        /// 商品重量
        /// </summary>
        public virtual double Weight { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public virtual String Platform { get; set; }

        /// <summary>
        /// 渠道账号
        /// </summary>
        public virtual String Account { get; set; }

        /// <summary>
        /// 销售站点
        /// </summary>
        public virtual String SalesSite { get; set; }

        /// <summary>
        /// 发货仓库
        /// </summary>
        public virtual String Warehouse { get; set; }

        /// <summary>
        /// 包裹号
        /// </summary>
        public virtual String PackageNo { get; set; }

        /// <summary>
        /// 邮寄方式
        /// </summary>
        public virtual String Logistics { get; set; }

        /// <summary>
        /// 包裹总重量
        /// </summary>
        public virtual double TotalWeight { get; set; }

        /// <summary>
        /// 包裹总运费
        /// </summary>
        public virtual Decimal TotalFreight { get; set; }

        /// <summary>
        /// 跟踪号
        /// </summary>
        public virtual String TrackCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public virtual String OrderNo { get; set; }

        /// <summary>
        /// ItemId
        /// </summary>
        public virtual String ItemId { get; set; }

        /// <summary>
        /// Item Title
        /// </summary>
        public virtual String ItemTitle { get; set; }

        /// <summary>
        /// 买家ID
        /// </summary>
        public virtual String BuyerId { get; set; }

        /// <summary>
        /// 买家姓名
        /// </summary>
        public virtual String BuyerName { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public virtual String Country { get; set; }

        /// <summary>
        /// 收货地址1
        /// </summary>
        public virtual String Address1 { get; set; }

        /// <summary>
        /// 收货地址2
        /// </summary>
        public virtual String Address2 { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public virtual String City { get; set; }

        /// <summary>
        /// 省/州
        /// </summary>
        public virtual String Province { get; set; }

        /// <summary>
        /// 邮编
        /// </summary>
        public virtual String PostCode { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public virtual String Phone { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public virtual String Telphone { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        public virtual String FullAddress { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public virtual DateTime PayOn { get; set; }

        /// <summary>
        /// 销售时间
        /// </summary>
        public virtual DateTime SaleOn { get; set; }

        /// <summary>
        /// 收款PayPal
        /// </summary>
        public virtual String CollectionPayPal { get; set; }

        /// <summary>
        /// 付款PayPal
        /// </summary>
        public virtual String PaymentPayPal { get; set; }

        /// <summary>
        /// 跟单员
        /// </summary>
        public virtual String Merchandiser { get; set; }

        /// <summary>
        /// 产品开发员
        /// </summary>
        public virtual String ProductDeveloper { get; set; }

        /// <summary>
        /// 询价员
        /// </summary>
        public virtual String InquiryClerk { get; set; }

        /// <summary>
        /// 采购员
        /// </summary>
        public virtual String PurchasingAgent { get; set; }

        /// <summary>
        /// 收款币种
        /// </summary>
        public virtual String CurrencyCode { get; set; }

        /// <summary>
        /// 订单总售价
        /// </summary>
        public virtual Decimal TotalPrice { get; set; }

        /// <summary>
        /// 订单总售价(人民币)
        /// </summary>
        public virtual Decimal TotalPriceRMB { get; set; }

        /// <summary>
        /// 售价
        /// </summary>
        public virtual Decimal Price { get; set; }

        /// <summary>
        /// 售价(人民币)
        /// </summary>
        public virtual Decimal PriceRMB { get; set; }

        /// <summary>
        /// 商品成本
        /// </summary>
        public virtual Decimal Cost { get; set; }

        /// <summary>
        /// 渠道成交费币种
        /// </summary>
        public virtual String PlatformDealCurrency { get; set; }

        /// <summary>
        /// 渠道成交费
        /// </summary>
        public virtual Decimal PlatformDealFee { get; set; }

        /// <summary>
        /// 渠道成交费(人民币)
        /// </summary>
        public virtual Decimal PlatformDealFeeRMB { get; set; }

        /// <summary>
        /// PayPal fee币种
        /// </summary>
        public virtual String PayPalFeeCurrency { get; set; }

        /// <summary>
        /// PayPal fee
        /// </summary>
        public virtual Decimal PayPalFee { get; set; }

        /// <summary>
        /// PayPal fee(人民币)
        /// </summary>
        public virtual Decimal PayPalFeeRMB { get; set; }

        /// <summary>
        /// 渠道费用
        /// </summary>
        public virtual Decimal PlatformFee { get; set; }

        /// <summary>
        /// 头程运输方式
        /// </summary>
        public virtual String FirstLegLogistics { get; set; }

        /// <summary>
        /// 头程运费
        /// </summary>
        public virtual Decimal FirstLegFreight { get; set; }

        /// <summary>
        /// 头程报关费
        /// </summary>
        public virtual Decimal FirstLegDeclarationFee { get; set; }

        /// <summary>
        /// 包装材料
        /// </summary>
        public virtual String PackingMaterial { get; set; }

        /// <summary>
        /// 包装费用
        /// </summary>
        public virtual Decimal PackagingFee { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public virtual Decimal Freight { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public virtual Decimal Profit { get; set; }

        /// <summary>
        /// 利润率
        /// </summary>
        public virtual double ProfitRate { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public virtual String Area { get; set; }

        /// <summary>
        /// 运费是否导入
        /// </summary>
        public virtual int IsFreight { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public virtual decimal ActualFreight { get; set; }
        /// <summary>
        /// 收汇是否已导入
        /// </summary>
        public virtual int IsFan { get; set; }
        /// <summary>
        /// 收汇
        /// </summary>
        public virtual decimal FanAmount { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public virtual string Operator { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public virtual string OperateTime { get; set; }
    }
}
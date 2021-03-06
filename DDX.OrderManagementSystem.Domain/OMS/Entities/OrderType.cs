﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// OrderType
    /// 订单表
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
    [Serializable]
    public class OrderType
    {
        public OrderType()
        {

        }
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 拆分订单主订单ID
        /// </summary>
        public virtual int MId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public virtual String OrderNo { get; set; }

        /// <summary>
        /// 外部编号
        /// </summary>
        public virtual String OrderExNo { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public virtual String Status { get; set; }

        /// <summary>
        /// FBA订单
        /// </summary>
        public virtual int IsFBA { get; set; }

        /// <summary>
        /// FBA订单
        /// </summary>
        public virtual string FBABy { get; set; }
        /// <summary>
        /// 是否打印
        /// </summary>
        public virtual int IsPrint { get; set; }

        /// <summary>
        /// 是否打印
        /// </summary>
        public virtual int IsLiu { get; set; }

        /// <summary>
        /// 是否上传到平台
        /// </summary>
        public virtual int IsUpload { get; set; }

        /// <summary>
        /// 问题订单
        /// </summary>
        public virtual int IsError { get; set; }

        /// <summary>
        /// 停售订单
        /// </summary>
        public virtual int IsStop { get; set; }

        /// <summary>
        /// 问题订单处理
        /// </summary>
        public virtual int IsAudit { get; set; }

        /// <summary>
        /// 合并订单
        /// </summary>
        public virtual int IsMerger { get; set; }

        /// <summary>
        /// 拆分订单
        /// </summary>
        public virtual int IsSplit { get; set; }

        /// <summary>
        /// 缺货订单
        /// </summary>
        public virtual int IsOutOfStock { get; set; }

        /// <summary>
        /// 重发订单
        /// </summary>
        public virtual int IsRepeat { get; set; }

        /// <summary>
        /// 虚假发货订单
        /// </summary>
        public virtual int IsXu { get; set; }


        /// <summary>
        /// 是否可以拆分订单
        /// </summary>
        public virtual int IsCanSplit { get; set; }

        /// <summary>
        /// Enabled
        /// </summary>
        public virtual int Enabled { get; set; }

        /// <summary>
        /// 货币
        /// </summary>
        public virtual String CurrencyCode { get; set; }

        /// <summary>
        /// 美金金额
        /// </summary>
        public virtual double Amount { get; set; }

        /// <summary>
        /// 原本金额
        /// </summary>
        public virtual double AmountOld { get; set; }


        public virtual String OrderCurrencyCode { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public virtual double OrderFees { get; set; }


        /// <summary>
        /// 货币
        /// </summary>
        public virtual String OrderCurrencyCode2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual double OrderFees2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual double Profit { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public virtual double ProductFees { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public virtual double RMB { get; set; }

        /// <summary>
        /// 流水交易号
        /// </summary>
        public virtual String TId { get; set; }

        /// <summary>
        /// 买家
        /// </summary>
        public virtual String BuyerName { get; set; }

        /// <summary>
        /// 买家邮箱
        /// </summary>
        public virtual String BuyerEmail { get; set; }

        /// <summary>
        /// 买家Id
        /// </summary>
        public virtual int BuyerId { get; set; }

        /// <summary>
        /// 买家留言
        /// </summary>
        public virtual String BuyerMemo { get; set; }

        /// <summary>
        /// 商家留言
        /// </summary>
        public virtual String SellerMemo { get; set; }

        /// <summary>
        /// 包裹截留留言
        /// </summary>
        public virtual String CutOffMemo { get; set; }

        /// <summary>
        /// 发货方式
        /// </summary>
        public virtual String LogisticMode { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public virtual String Country { get; set; }

        /// <summary>
        /// 地址Id
        /// </summary>
        public virtual int AddressId { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public virtual int Weight { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public virtual int Weight2 { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public virtual double Freight { get; set; }

        /// <summary>
        /// 运费是否导入
        /// </summary>
        public virtual int IsFreight { get; set; }

        /// <summary>
        /// 运输条码
        /// </summary>
        public virtual string TrackCode { get; set; }

        /// <summary>
        /// 运输条码
        /// </summary>
        public virtual string TrackCode2 { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public virtual DateTime GenerateOn { get; set; }

        /// <summary>
        /// 同步时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// 扫描时间
        /// </summary>
        public virtual DateTime ScanningOn { get; set; }

        /// <summary>
        /// 扫描人
        /// </summary>
        public virtual String ScanningBy { get; set; }

        /// <summary>
        /// 放款状态
        /// </summary>
        public virtual int FanState { get; set; }

        /// <summary>
        /// 放款时间
        /// </summary>
        public virtual DateTime? FanDate { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public virtual decimal FanAmount { get; set; }

        /// <summary>
        /// 预计毛利润率
        /// </summary>
        public virtual decimal ReProfitRate { get; set; }

        /// <summary>
        /// 实际毛利润率
        /// </summary>
        public virtual decimal ProfitRate { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public virtual double ReFanAmount { get; set; }

        /// <summary>
        /// 赔款
        /// </summary>
        public virtual decimal PeiKuan { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public virtual String Account { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public virtual String Platform { get; set; }
        /// <summary>
        /// 订单的产品
        /// </summary>

        public virtual List<OrderProductType> Products { get; set; }
        /// <summary>
        /// 订单拣货单关联表
        /// </summary>
        public virtual List<OrderPackageType> OrderPack { get; set; }
        /// <summary>
        /// 订单的地址
        /// </summary>

        public virtual OrderAddressType AddressInfo { get; set; }

        /// <summary>
        /// 订单格式错误留言
        /// </summary>
        public virtual String ErrorInfo { get; set; }

        public virtual string rows { get; set; }

        public virtual int qty { get; set; }
        /// <summary>
        /// 延时允许发货
        /// </summary>
        public virtual int AllowDelivery { get; set; }
        /// <summary>
        /// 订单是否允许发货，即1.超过15天而且延时允许发货字段是1 2.不超过15天
        /// </summary>
        public virtual bool IsAllowDelivery { get; set; }
        /// <summary>
        /// 是否扣库存
        /// </summary>
        public virtual int IsminusStock { get; set; }
        /// <summary>
        /// 拣货单ID
        /// </summary>
        public virtual int PickID { get; set; }
        /// <summary>
        /// 客户无理取闹
        /// </summary>
        public virtual double BuyUnreason { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public virtual String Province { get; set; }

        /// <summary>
        /// 是否导出标签
        /// </summary>
        public virtual int IsDao { get; set; }

    }
}

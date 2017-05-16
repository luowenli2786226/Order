//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ShipmentsType
    /// 出货明细表
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
    public class ShipmentsType
    {
        /// <summary>
        /// 出货明细Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 英文描述
        /// </summary>
        public virtual String DescribeEn { get; set; }

        /// <summary>
        /// 中文描述
        /// </summary>
        public virtual String DescribeCn { get; set; }

        /// <summary>
        /// 我司货号（SKU）
        /// </summary>
        public virtual String Sku { get; set; }

        /// <summary>
        /// 采购合同编号
        /// </summary>
        public virtual String PurchaseNo { get; set; }

        /// <summary>
        /// 外销合同号码
        /// </summary>
        public virtual String ExportNo { get; set; }

        /// <summary>
        /// UMAX价格USD
        /// </summary>
        public virtual String PriceUMax { get; set; }

        /// <summary>
        /// 工厂价格
        /// </summary>
        public virtual String PriceFactory { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public virtual String Unit { get; set; }

        /// <summary>
        /// 包装
        /// </summary>
        public virtual int PackageNo { get; set; }

        /// <summary>
        /// 箱数
        /// </summary>
        public virtual int Ctn { get; set; }

        /// <summary>
        /// 是否已报关
        /// </summary>
        public virtual Boolean IsCustoms { get; set; }

        /// <summary>
        /// 毛重（KG）
        /// </summary>
        public virtual double WeightGross { get; set; }

        /// <summary>
        /// 净重（KG）
        /// </summary>
        public virtual double WeightNet { get; set; }

        /// <summary>
        /// 总体积
        /// </summary>
        public virtual double TotalVolume { get; set; }

        /// <summary>
        /// 实际数量
        /// </summary>
        public virtual double Qty { get; set; }
        /// <summary>
        /// 第一次创建时候计算出来的数量
        /// </summary>
        public virtual double FirstQty { get; set; }

        /// <summary>
        /// 客人含佣金总价
        /// </summary>
        public virtual double TotalPrice { get; set; }

        /// <summary>
        /// 增值税率
        /// </summary>
        public virtual double TaxRate { get; set; }

        /// <summary>
        /// 比值
        /// </summary>
        public virtual double Ratio { get; set; }

        /// <summary>
        /// 业务人
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// 采购员
        /// </summary>
        public virtual String CreatePlanBy { get; set; }

        /// <summary>
        /// 跟单员
        /// </summary>
        public virtual String CreatetTrackBy { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public virtual String Paymethod { get; set; }

        /// <summary>
        /// 头程费用(元/公斤)
        /// </summary>
        public virtual String HeadloadCharges { get; set; }

        /// <summary>
        /// 优胜价格
        /// </summary>
        public virtual double YouShengPrice { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }
        /// <summary>
        /// 出货清单ID
        /// </summary>
        public virtual int ShipmentslistId { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public virtual string UpdateBy { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTime UpdateTime { get; set; }
        /// <summary>
        /// 单价(总)
        /// </summary>
        public virtual double Price { get; set; }

        /// 优胜价格(总)
        /// </summary>
        public virtual double YouShengtotal { get; set; }

        /// 工厂总价(总)
        /// </summary>
        public virtual double PriceFactorytotal { get; set; }

        /// 毛重(总)
        /// </summary>
        public virtual double WeightGrosstotal { get; set; }

        /// 净重(总)
        /// </summary>
        public virtual double WeightNettotal { get; set; }

        /// 单个SKU头程$
        /// </summary>
        public virtual double HeadloadCharges1 { get; set; }

    }
}

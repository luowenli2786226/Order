//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PlacardType
    /// 公告管理
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
    public class PickingListType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// SKU种类
        /// </summary>
        public virtual int SkuCategory { get; set; }
        /// <summary>
        /// 拣货单号
        /// </summary>
        public virtual int PickingNo { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public virtual string PickingType { get; set; }

        /// <summary>
        /// 订单数
        /// </summary>
        public virtual int OrderCount { get; set; }
     
        /// <summary>
        /// 已打印的包裹个数
        /// </summary>
        public virtual string PrintOrderCount { get; set; }
        /// <summary>
        /// 已打印的货品
        /// </summary>
        public virtual string ScanProduct { get; set; }
        /// <summary>
        /// 产品数量
        /// </summary>
        public virtual int SKUcount { get; set; }

        /// <summary>
        /// 拣货单状态
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public virtual string CreateBy { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public virtual string WareHouse { get; set; }
        /// <summary>
        /// 邮寄方式
        /// </summary>
        public virtual string LogisticMode { get; set; }
        /// <summary>
        /// 作业开始时间
        /// </summary>
        public virtual DateTime BeginWorkTime { get; set; }
        /// <summary>
        /// 作业时长
        /// </summary>
        public virtual string WorkTimeLength { get; set; }
        /// <summary>
        /// 小组成员
        /// </summary>
        public virtual string Partner { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual List<OrderPackageType> orderpack { get; set; }
    }
    public class PickingLogicsType
    {
        /// <summary>
        /// 订单数量
        /// </summary>
        public  int OrderCount2 { get; set; }
        /// <summary>
        /// 邮寄方式
        /// </summary>
        public string LogisticsMode2 { get; set; }
    }
}

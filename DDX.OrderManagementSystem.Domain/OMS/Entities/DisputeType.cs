//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DisputeType
    /// 纠纷表
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
    public class DisputeRecordType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// CreateOn
        /// </summary>
        public virtual DateTime CreateOn { get; set; }


        /// <summary>
        /// CreateOn
        /// </summary>
        public virtual DateTime? PayOn { get; set; }

        /// <summary>
        /// CreateOn
        /// </summary>
        public virtual decimal Rate { get; set; }


        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String ZeRenBy { get; set; }


        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String Area { get; set; }
        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String DisputeState { get; set; }


        /// <summary>
        /// ExamineBy
        /// </summary>
        public virtual String ExamineBy { get; set; }

        /// <summary>
        /// ExamineOn
        /// </summary>
        public virtual DateTime ExamineOn { get; set; }

        /// <summary>
        /// ExamineTitle
        /// </summary>
        public virtual String ExamineTitle { get; set; }


        /// <summary>
        /// ExamineStatus
        /// </summary>
        public virtual int ExamineStatus { get; set; }

        /// <summary>
        /// ExamineMemo
        /// </summary>
        public virtual String ExamineMemo { get; set; }

        /// <summary>
        /// ExamineType
        /// </summary>
        public virtual String ExamineType { get; set; }


        /// <summary>
        /// ExamineClass
        /// </summary>
        public virtual String ExamineClass { get; set; }

        /// <summary>
        /// ExamineHandle
        /// </summary>
        public virtual String ExamineHandle { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public virtual String OrderNo { get; set; }


        /// <summary>
        /// 店铺
        /// </summary>
        public virtual String Account { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public virtual String Platform { get; set; }

        /// <summary>
        /// 预赔金额
        /// </summary>
        public virtual Decimal OrderAmount { get; set; }


        /// <summary>
        /// 订单金额
        /// </summary>
        public virtual Decimal OrderAmount2 { get; set; }

        /// <summary>
        /// sku
        /// </summary>
        public virtual String SKU { get; set; }
        /// <summary>
        /// ExamineAmount
        /// </summary>
        public virtual Decimal ExamineAmount { get; set; }
        /// <summary>
        /// 实赔人民币
        /// </summary>
        public virtual Decimal ExamineAmountRmb { get; set; }

        /// <summary>
        /// ExamineCurrencyCode
        /// </summary>
        public virtual String ExamineCurrencyCode { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public virtual String Remark { get; set; }
        /// <summary>
        /// 区分是否是导入的纠纷数据
        /// </summary>
        public virtual int IsImport { get; set; }
        /// <summary>
        /// 退款时间
        /// </summary>
        public virtual string RefundDate { get; set; }

        /// <summary>
        /// paypal账号
        /// </summary>
        public virtual string Paypal { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public virtual string ImgPic { get; set; }

    }
}

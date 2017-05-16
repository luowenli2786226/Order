//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C) 2018 , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ServiceExaminationType
    /// 售后审批
    /// 
    /// 修改纪录
    /// 
    /// 1.19 版本：1.0 XiDong 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDong</name>
    /// <date>1.19</date>
    /// </author>
    /// </summary>
    public class ServiceExaminationType
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
        /// CreateBy
        /// </summary>
        public virtual String CreateBy { get; set; }
        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String Area { get; set; }

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
        /// 订单金额
        /// </summary>
        public virtual Decimal OrderAmount { get; set; }


        /// <summary>
        /// ExamineAmount
        /// </summary>
        public virtual Decimal ExamineAmount { get; set; }

        /// <summary>
        /// ExamineCurrencyCode
        /// </summary>
        public virtual String ExamineCurrencyCode { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public virtual String Content { get; set; }

    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PurchasePlanExamineRecordType
    /// 采购计划审批记录
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
    public class PurchasePlanExamineRecordType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 审批名称
        /// </summary>
        public virtual String ExamineTitle { get; set; }

        /// <summary>
        /// 审批类型
        /// </summary>
        public virtual String ExamineType { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public virtual DateTime ExamineOn { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public virtual DateTime BeginDate { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public virtual DateTime EndDate { get; set; }

        /// <summary>
        /// 审批人
        /// </summary>
        public virtual String ExamineBy { get; set; }

        /// <summary>
        /// 审批评语
        /// </summary>
        public virtual String ExamineContent { get; set; }

        /// <summary>
        /// 悲剧
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 审批状态
        /// </summary>
        public virtual int ExamineStatus { get; set; }

        /// <summary>
        /// 审批总金额
        /// </summary>
        public virtual double ExamineAmount { get; set; }

        /// <summary>
        /// 审批总金额
        /// </summary>
        public virtual decimal TuiAmount { get; set; }

        /// <summary>
        /// 审批总计划数
        /// </summary>
        public virtual int ExamineCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public virtual String CreateBy { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public virtual String Area { get; set; }
        /// <summary>
        /// 采购模式
        /// </summary>
        public virtual string ProcurementModel { get; set; }
    }
}

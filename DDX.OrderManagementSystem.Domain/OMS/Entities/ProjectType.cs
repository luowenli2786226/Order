//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ProjectType
    /// 项目表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 Xidong 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>Xidong</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class ProjectType
    {
        /// <summary>
        /// 主键标识
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 项目标题
        /// </summary>
        public virtual String Title { get; set; }

        /// <summary>
        /// 项目内容
        /// </summary>
        public virtual String Content { get; set; }

        /// <summary>
        /// 项目状态
        /// </summary>
        public virtual int State { get; set; }

        /// <summary>
        /// 项目备注
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime? CreateOn { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public virtual String AuditBy { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public virtual DateTime? AuditOn { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public virtual String AuditRemark { get; set; }

        /// <summary>
        /// 最近开发进度
        /// </summary>
        public virtual String LastState { get; set; }

        /// <summary>
        /// 最近更新时间
        /// </summary>
        public virtual DateTime? LastOn { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public virtual int IsAudit { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public virtual DateTime? BeginDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public virtual DateTime? EndDate { get; set; }

        /// <summary>
        /// 评价
        /// </summary>
        public virtual String EvaluateContent { get; set; }

    }
}

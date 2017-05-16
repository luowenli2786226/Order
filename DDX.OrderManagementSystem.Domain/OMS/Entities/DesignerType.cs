//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// AccountType
    /// 平台账户表
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
    public class DesignerType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int DesignerID { get; set; }

        public virtual string Contentitle { get; set; }//项目内容

        public virtual DateTime? Lasttime { get; set; }//截止日期

        public virtual string Apllicant { get; set; }//申请人

        public virtual DateTime? Apllicantime { get; set; }//申请时间

        public virtual string Auditor { get; set; }//审核人

        public virtual DateTime ? Audittime { get; set; }//审核时间

        public virtual string Auditnotes { get; set; }//审核备注

        public virtual string Auditstatus { get; set; }//审核状态

        public virtual DateTime? Expectedtime { get; set; }//预计完成时间

        public virtual string Receivenotes { get; set; }//领取备注

        public virtual string Receiptor { get; set; }//领取人

        public virtual string Receivingsate { get; set; }//领取状态

        public virtual  DateTime? Receivingtime { get; set; }//领取时间

        public virtual DateTime? Finishtime { get; set; }//完成时间
    }
}

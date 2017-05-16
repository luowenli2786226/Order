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
    public class WishDisputeRecordType
    {
        public virtual int Id { get; set; }
        public virtual string Area { get; set; }
        public virtual string Account { get; set; }
        public virtual string OrderNo { get; set; }
        public virtual string DisputeState { get; set; }
        public virtual decimal ExamineAmount { get; set; }
        public virtual string CreateBy { get; set; }
        public virtual DateTime CreateOn { get; set; }
        public virtual string ExamineClass { get; set; }
        public virtual string ExamineStatus { get; set; }
        public virtual DateTime ExamineOn { get; set; }
    }
}

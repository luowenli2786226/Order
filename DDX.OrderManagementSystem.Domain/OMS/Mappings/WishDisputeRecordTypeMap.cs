//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// AccountEmailTypeMap
    /// 账户邮件
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
    public class WishDisputeTypeMap : ClassMap<WishDisputeRecordType>
    {
        public WishDisputeTypeMap()
        {
            Table("WishDisputeRecord");
            Id(x => x.Id);
            Map(x => x.Area).Length(50);
            Map(x => x.Account).Length(50);
            Map(x => x.OrderNo).Length(200);
            Map(x => x.DisputeState).Length(50);
            Map(x => x.ExamineAmount);
            Map(x => x.CreateBy);
            Map(x => x.CreateOn);
            Map(x => x.ExamineClass);
            Map(x => x.ExamineStatus);
            Map(x => x.ExamineOn);
        }
    }
}

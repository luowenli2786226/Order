//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// PurchasePlanExamineRecordTypeMap
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
    public class PurchasePlanExamineRecordTypeMap : ClassMap<PurchasePlanExamineRecordType>
    {
        public PurchasePlanExamineRecordTypeMap()
        {
            Table("PurchasePlanExamineRecord");
            Id(x => x.Id);
            Map(x => x.ExamineTitle).Length(300);
            Map(x => x.ExamineType).Length(300);
            Map(x => x.ExamineOn);
            Map(x => x.ExamineBy).Length(300);
            Map(x => x.ExamineContent).Length(3000);
            Map(x => x.ExamineStatus);
            Map(x => x.ExamineAmount).Length(18);
            Map(x => x.ExamineCount);
            Map(x => x.CreateOn);
            Map(x => x.BeginDate);
            Map(x => x.EndDate);
            Map(x => x.TuiAmount);
            
            Map(x => x.Area);
            Map(x => x.Remark).Length(1000);
            Map(x => x.CreateBy).Length(300);
            Map(x => x.ProcurementModel).Length(50);
        }
    }
}

//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C) 2018 , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ServiceExaminationTypeMap
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
    public class ServiceExaminationTypeMap : ClassMap<ServiceExaminationType>
    {
        public ServiceExaminationTypeMap()
        {
            Table("ServiceExamination");
            Id(x => x.Id);
            Map(x => x.CreateOn);
            Map(x => x.CreateBy).Length(200);
            Map(x => x.ExamineBy).Length(200);
            Map(x => x.ExamineOn);
            Map(x => x.PayOn);
            Map(x => x.ExamineStatus);
            Map(x => x.Area);
            Map(x => x.Account);
            Map(x => x.OrderAmount);
            Map(x => x.OrderNo);
            Map(x => x.ExamineClass).Length(400);
            Map(x => x.ExamineHandle).Length(400);
            Map(x => x.ExamineTitle).Length(200);
            Map(x => x.ExamineMemo).Length(800);
            Map(x => x.ExamineType).Length(20);
            Map(x => x.ExamineAmount).Length(18);
            Map(x => x.ExamineCurrencyCode).Length(20);
            Map(x => x.Remark).Length(2000);
            Map(x => x.Content).Length(2000);
        }
    }
}

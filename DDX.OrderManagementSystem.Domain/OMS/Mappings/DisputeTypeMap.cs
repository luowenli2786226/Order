//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DisputeTypeMap
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
    public class DisputeRecordTypeMap : ClassMap<DisputeRecordType>
    {
        public DisputeRecordTypeMap()
        {
            Table("DisputeRecordType");

            Id(x => x.Id);
            Map(x => x.CreateOn);
            Map(x => x.Rate);
            Map(x => x.ZeRenBy);
            Map(x => x.DisputeState);
            Map(x => x.SKU);
            Map(x => x.CreateBy).Length(200);
            Map(x => x.ExamineBy).Length(200);
            Map(x => x.ExamineOn);
            Map(x => x.PayOn);
            Map(x => x.ExamineStatus);
            Map(x => x.ExamineAmountRmb);
            Map(x => x.Area);
            Map(x => x.Account);
            Map(x => x.OrderAmount);
            Map(x => x.OrderAmount2);
            Map(x => x.OrderNo);
            Map(x => x.ExamineClass).Length(400);
            Map(x => x.ExamineHandle).Length(400);
            Map(x => x.ExamineTitle).Length(200);
            Map(x => x.ExamineMemo).Length(800);
            Map(x => x.ExamineType).Length(20);
            Map(x => x.ExamineAmount).Length(18);
            Map(x => x.ExamineCurrencyCode).Length(20);
            Map(x => x.Remark).Length(2000);
            Map(x => x.Platform).Length(200);
            Map(x => x.IsImport);
            Map(x => x.RefundDate);
            Map(x => x.Paypal).Length(200);
            Map(x => x.ImgPic);

        }
    }
}

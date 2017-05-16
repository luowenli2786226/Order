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
    public class OtherExpensesTypeMap : ClassMap<OtherExpensesType>
    {
        public OtherExpensesTypeMap()
        {
            Table("OtherExpenses");
            Id(x => x.Id);
            Map(x => x.Platform).Length(50);
            Map(x => x.Account).Length(50);
            Map(x => x.Currency).Length(50);
            Map(x => x.Amount);
            Map(x => x.Remarks).Length(2000);
            Map(x => x.ProcessDate);
        }
    }
}

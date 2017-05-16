//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// FixedRateTypeMap
    /// 固定汇率
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
    public class FixedRateTypeMap : ClassMap<FixedRateType>
    {
        public FixedRateTypeMap()
        {
            Table("FixedRate");
            Id(x => x.Id);
            Map(x => x.CurrencyName).Length(30);
            Map(x => x.CurrencyCode).Length(30);
            Map(x => x.CurrencyValue);
            Map(x => x.Year);
            Map(x => x.Month);
            Map(x => x.CreateBy);
            Map(x => x.CreateOn);
        }
    }
}

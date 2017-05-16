using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ParentCountryTypeMap : ClassMap<ParentCountryType> 
    {
        public ParentCountryTypeMap()
        {
            Table("ParentCountry");
            Id(x => x.Id);
            Map(x => x.Name);
        }
    }
}

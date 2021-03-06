﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , Dean TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DataDictionaryDetailTypeMap
    /// 数据字典明细
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
    public class DataDictionaryDetailTypeMap : ClassMap<DataDictionaryDetailType>
    {
        public DataDictionaryDetailTypeMap()
        {
            Table("DataDictionaryDetails");
            Id(x => x.Id);
            Map(x => x.DicCode).Length(50);
            Map(x => x.FullName).Length(50);
            Map(x => x.DicValue).Length(50);
            Map(x => x.DicValue2).Length(50);
            Map(x => x.GroupBy).Length(50);
            Map(x => x.AllowDelete);
        }
    }
}

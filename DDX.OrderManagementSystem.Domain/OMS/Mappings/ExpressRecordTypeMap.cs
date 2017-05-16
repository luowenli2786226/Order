//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ExpressRecordTypeMap
    /// 快递记录
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 XiDong 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDong</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class ExpressRecordTypeMap : ClassMap<ExpressRecordType> 
    {
        public ExpressRecordTypeMap()
        {
            Table("ExpressRecord");
            Id(x => x.Id);
            Map(x => x.TrackCode).Length(200);
            Map(x => x.CreateBy).Length(200);
            Map(x => x.CreateOn);
            Map(x => x.IsVail);
        }
    }
}

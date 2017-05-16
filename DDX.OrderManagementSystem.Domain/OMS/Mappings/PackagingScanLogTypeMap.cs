//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//---
using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// PackagingScanLogTypeMap
    /// 包装扫描日志表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0  创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// 版本：1.1  增加订单号。
    /// 
    /// 版本1.1
    /// 
    /// <author>
    /// <name></name>
    /// <date></date>
    /// </author>
    /// </summary>
    class PackagingScanLogTypeMap : ClassMap<PackagingScanLogType>
    {
        public PackagingScanLogTypeMap()
        {
            Table("PackagingScanLog");
            Id(x => x.Id);
            Map(x => x.PackageType).Length(50);
            Map(x => x.Operator).Length(20);
            Map(x => x.OperationOn);
            Map(x => x.OrderNo).Length(40); ;
        }
    }
}

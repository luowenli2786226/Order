﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// MachinTypeMap
    /// 设备管理
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
    public class MachineTypeMap : ClassMap<MachineType> 
    {
        public MachineTypeMap()
        {
            Table("Machine");
            Id(x => x.Id);
            Map(x => x.Code).Length(50);
            Map(x => x.MachineClass).Length(50);
            Map(x => x.Name).Length(50);
            Map(x => x.Status).Length(50);
            Map(x => x.UserName).Length(50);
            Map(x => x.StartDate);
            Map(x => x.EndDate);
            Map(x => x.BuyDate);
            Map(x => x.BuyMoney).Length(50);
            Map(x => x.BuyBy).Length(50);
        }
    }
}

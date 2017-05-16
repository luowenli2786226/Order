﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// WarehouseProInfoTypeMap
    /// 库位表
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
    public class WarehouseProInfoTypeMap : ClassMap<WarehouseProInfoType>
    {
      public WarehouseProInfoTypeMap()
      {
          Table("WarehouseProInfo");
          Id(x => x.Id);
          Map(x => x.WareHouse).Length(50);
          Map(x => x.Province).Length(50);
          Map(x => x.Orderby);
          
      }
    }
}

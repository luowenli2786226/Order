using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{
  public  class OrderPackageTypeMap:ClassMap<OrderPackageType>
    {
      public OrderPackageTypeMap()
      {
          Table("OrderPackage");
          Id(x => x.Id);
          Map(x => x.OrderNo);
          Map(x => x.IsPrint);
          Map(x => x.PickingNo);
      }
    }
}

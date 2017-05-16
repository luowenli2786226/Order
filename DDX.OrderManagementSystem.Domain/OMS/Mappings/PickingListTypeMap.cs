using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
namespace DDX.OrderManagementSystem.Domain.OMS
{
   public class PickingListTypeMap:ClassMap<PickingListType>
    {
       public PickingListTypeMap()
       {
           Table("PickingList");
           Id(x => x.Id);
           Map(x => x.PickingNo);
           Map(x => x.OrderCount);
           Map(x => x.SkuCategory);
           Map(x => x.PickingType).Length(100);
           Map(x => x.SKUcount);
           Map(x => x.State).Length(100);
           Map(x => x.CreateTime);
           Map(x => x.CreateBy).Length(100);
           Map(x => x.WareHouse).Length(100);
           Map(x => x.LogisticMode).Length(100);
           Map(x => x.BeginWorkTime);
           Map(x => x.WorkTimeLength).Length(100);
           Map(x => x.Partner).Length(100);
       }
    }
}

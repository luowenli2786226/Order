using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    public class GoodsDiscountRecordTypeMap : ClassMap<GoodsDiscountRecordType> 
    {
        public GoodsDiscountRecordTypeMap()
        {
            Table("GoodsDiscountRecord");
            Id(x => x.GoodsDiscountRecordId);
            Map(x => x.DepositoryInId);
            Map(x => x.GoodsId);
            Map(x => x.DicCycleType).Length(20);
            Map(x => x.BeforeUnitPrice);
            Map(x => x.DiscountRate);
            Map(x => x.DiscountQty);
            Map(x => x.DiscountCycle);
            Map(x => x.GmtCreate);
        }
    }
}

using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    public class GoodsDiscountRulesTypeMap : ClassMap<GoodsDiscountRulesType> 
    {
        public GoodsDiscountRulesTypeMap()
        {
            Table("GoodsDiscountRules");
            Id(x => x.GoodsDiscountRulesId);
            Map(x => x.DicCycleType).Length(20);
            Map(x => x.DiscountCycleBegin);
            Map(x => x.DiscountCycleEnd);
            Map(x => x.DiscountRate);
            Map(x => x.WarningColor);
        }
    }
}

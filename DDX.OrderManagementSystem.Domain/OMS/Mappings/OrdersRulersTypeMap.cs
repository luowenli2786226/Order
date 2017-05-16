using DDX.OrderManagementSystem.Domain.OMS.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    public class OrdersRulersTypeMap : ClassMap<OrdersRulersType> 
    {
        public OrdersRulersTypeMap()
        {
            Table("OrdersRulers");
            Id(x=>x.Id);
            Map(x=>x.Logictis).Length(200);
            Map(x => x.WareHouse).Length(50);
            Map(x => x.OrderAction).Length(50);
            Map(x => x.OrderChoose);
            Map(x => x.RulersName).Length(100);
            Map(x => x.IsMinusStock);
            Map(x => x.IsUse);
            Map(x => x.Priority);
        }
    }
}

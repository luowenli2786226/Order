using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using DDX.OrderManagementSystem.Domain.OMS.Entities;

namespace DDX.OrderManagementSystem.Domain.OMS.Mappings
{
    public class WarehouseTurnoverMap : ClassMap<WarehouseTurnoverType>
    {
        public WarehouseTurnoverMap()
        {
            Table("WarehouseTurnover");
            Id(x => x.TurnoverId);
            Map(x => x.LocationCode);
            Map(x => x.SkuCode);
            Map(x => x.Status);
            Map(x => x.Qty);
            Map(x => x.CreateBy);
            Map(x => x.CreateOn);
        }
    }
}

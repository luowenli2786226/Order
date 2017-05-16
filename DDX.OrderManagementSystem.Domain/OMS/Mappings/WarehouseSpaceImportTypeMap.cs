using FluentNHibernate.Mapping;
using System;
using System.Linq.Expressions;

namespace DDX.OrderManagementSystem.Domain
{
    public class WarehouseSpaceImportTypeMap : ClassMap<WarehouseSpaceImportType>
    {
        public WarehouseSpaceImportTypeMap()
        {
            Table("WarehouseSpaceImport");
            Id(x => x.Id);
            Map(x => x.PId);
            Map(x => x.Qty);
            Map(x => x.SId);
            Map(x => x.Status);
            Map(x => x.UpdateOn);
            Map(x => x.CreateBy).Length(50);
        }
    }
}

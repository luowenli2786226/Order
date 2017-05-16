using DDX.OrderManagementSystem.Domain.OMS.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain.OMS.Mappings
{
    public class ConfigTempleTypeMap : ClassMap<ConfigTempleType>
    {
        public ConfigTempleTypeMap()
        {
            Table("ConfigTemple");
            Id(x => x.ID);
            Map(x => x.Category).Length(20);
            Map(x => x.Price);
            Map(x => x.ParamValue);
        }
    }
}

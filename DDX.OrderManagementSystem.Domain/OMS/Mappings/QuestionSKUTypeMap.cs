using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{
    public class QuestionSKUTypeMap : ClassMap<QuestionSKUType>
    {

        public QuestionSKUTypeMap()
        {
            Table("QuestionSKU");
            Id(x => x.Id);
            Map(x => x.SKU).Length(100);
            Map(x => x.Memo).Length(400);
            Map(x => x.CreateBy);
            Map(x => x.CreateOn);
        }
    }
}

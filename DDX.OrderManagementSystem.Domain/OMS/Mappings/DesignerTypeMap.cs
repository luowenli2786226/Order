//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace DDX.OrderManagementSystem.Domain.OMS.Mappings
//{
//    class DesignerTypeMap
//    {
//    }
//}


using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DesignerTypeMap
    /// 美工表
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
    public class DesignerTypeMap : ClassMap<DesignerType>
    {
        public DesignerTypeMap()
        {
            Table("Designers");
            Id(x => x.DesignerID);
            Map(x => x.Contentitle).Length(800);
            Map(x => x.Lasttime).Nullable();
            Map(x => x.Apllicant).Length(200);
            Map(x => x.Apllicantime).Nullable();
            Map(x => x.Auditor).Length(200);
            Map(x => x.Audittime).Nullable();
            Map(x => x.Auditnotes).Length(800);
            Map(x => x.Auditstatus).Length(800);
            Map(x => x.Expectedtime).Nullable();
            Map(x => x.Receiptor).Length(200);
            Map(x => x.Receivenotes).Length(800);
            Map(x => x.Receivingsate).Length(800);
            Map(x => x.Receivingtime).Nullable();
            Map(x => x.Finishtime).Nullable();
           
        }
    }
}
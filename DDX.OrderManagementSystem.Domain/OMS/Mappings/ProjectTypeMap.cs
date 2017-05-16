//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ProjectTypeMap
    /// 项目表
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 Xidong 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>Xidong</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class ProjectTypeMap : ClassMap<ProjectType> 
    {
        public ProjectTypeMap()
        {
            Table("Projects");
            Id(x => x.Id);
            Map(x => x.Title).Length(800);
            Map(x => x.Content).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.State);
            Map(x => x.Remark).Length(800);
            Map(x => x.CreateBy).Length(200);
            Map(x => x.CreateOn);
            Map(x => x.AuditBy).Length(200);
            Map(x => x.AuditOn);
            Map(x => x.AuditRemark).Length(800);
            Map(x => x.LastState).Length(800);
            Map(x => x.LastOn);
            Map(x => x.IsAudit);
            Map(x => x.BeginDate);
            Map(x => x.EndDate);
            Map(x => x.EvaluateContent).Length(800);
        }
    }
}

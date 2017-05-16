//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ProjectStateTypeMap
    /// 项目进度
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
    public class ProjectStateTypeMap : ClassMap<ProjectStateType> 
    {
        public ProjectStateTypeMap()
        {
            Table("ProjectState");
            Id(x => x.Id);
            Map(x => x.PId);
            Map(x => x.CreateOn);
            Map(x => x.Content).Length(800);
            Map(x => x.CreateBy).Length(200);
        }
    }
}

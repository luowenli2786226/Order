//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ProjectStateType
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
    public class ProjectStateType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public virtual int PId { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public virtual String Content { get; set; }

        /// <summary>
        /// 添加人
        /// </summary>
        public virtual String CreateBy { get; set; }

    }
}

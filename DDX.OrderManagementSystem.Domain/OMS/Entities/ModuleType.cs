﻿//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ModuleType
    /// 模块（菜单）表
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
    public class ModuleType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 父节点主键
        /// </summary>
        public virtual int ParentId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public virtual String Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public virtual String FullName { get; set; }

        /// <summary>
        /// 图标编号
        /// </summary>
        public virtual String ImageIndex { get; set; }

        /// <summary>
        /// 导航地址
        /// </summary>
        public virtual String NavigateUrl { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public virtual int SortCode { get; set; }

        /// <summary>
        /// 删除标志
        /// </summary>
        public virtual int DeletionStateCode { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public virtual String Description { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public virtual String CreateBy { get; set; }

        public virtual List<ModuleType> children { get; set; }

    }
}

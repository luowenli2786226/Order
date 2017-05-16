//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , Dean TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// DataDictionaryDetailType
    /// 数据字典明细
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0  创建主键。T
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name></name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class DataDictionaryDetailType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 分类代码
        /// </summary>
        public virtual String DicCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public virtual String FullName { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public virtual String DicValue { get; set; }

        /// <summary>
        /// 值2
        /// </summary>
        public virtual String DicValue2 { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        public virtual String GroupBy { get; set; }

        /// <summary>
        /// 内置
        /// </summary>
        public virtual int AllowDelete { get; set; }
        /// <summary>
        /// 获取子类信息
        /// </summary>
        public virtual List<DataDictionaryDetailType> children { get; set; }

    }
}

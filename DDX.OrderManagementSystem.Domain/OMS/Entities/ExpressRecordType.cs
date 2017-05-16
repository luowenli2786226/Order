//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// ExpressRecordType
    /// 快递记录
    /// 
    /// 修改纪录
    /// 
    ///  版本：1.0 XiDong 创建主键。
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    /// <name>XiDong</name>
    /// <date></date>
    /// </author>
    /// </summary>
    public class ExpressRecordType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// TrackCode
        /// </summary>
        public virtual String TrackCode { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual String CreateBy { get; set; }

        /// <summary>
        /// CreateOn
        /// </summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>
        /// IsVail
        /// </summary>
        public virtual int IsVail { get; set; }

    }
}

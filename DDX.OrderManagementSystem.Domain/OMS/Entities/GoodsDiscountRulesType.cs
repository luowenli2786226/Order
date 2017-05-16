using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// GoodsDiscountRulesType
    /// 库损规则
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
    public class GoodsDiscountRulesType
    {
        /// <summary>
        /// 主键
        /// </summary>
        public virtual int GoodsDiscountRulesId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public virtual string DicCycleType { get; set; }
        /// <summary>
        /// 距当前时间月份差的开始范围
        /// </summary>
        public virtual int DiscountCycleBegin { get; set; }
        /// <summary>
        /// 距当前时间月份差的结束范围
        /// </summary>
        public virtual int DiscountCycleEnd { get; set; }
        /// <summary>
        /// 折损率
        /// </summary>
        public virtual decimal DiscountRate { get; set; }
        /// <summary>
        /// 单价背景色
        /// </summary>
        public virtual string WarningColor { get; set; }
    }
}

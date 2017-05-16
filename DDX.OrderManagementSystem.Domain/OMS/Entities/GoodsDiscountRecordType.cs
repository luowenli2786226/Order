using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// GoodsDiscountRecordType
    /// 库损记录
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
   public class GoodsDiscountRecordType
    {
       /// <summary>
       /// 主键
       /// </summary>
       public virtual int GoodsDiscountRecordId { get; set; }
       /// <summary>
       /// 到货批次
       /// </summary>
       public virtual int DepositoryInId { get; set; }
       /// <summary>
       /// 
       /// </summary>
       public virtual int GoodsId{get;set;}
       /// <summary>
       /// 
       /// </summary>
       public virtual int DicCycleType { get; set; }
       /// <summary>
       /// 折前单价
       /// </summary>
       public virtual decimal BeforeUnitPrice { get; set; }
       /// <summary>
       /// 折损率
       /// </summary>
       public virtual float DiscountRate { get; set; }
       /// <summary>
       /// 库损数量
       /// </summary>
       public virtual float DiscountQty { get; set; }
       /// <summary>
       /// 库损周期
       /// </summary>
       public virtual int DiscountCycle { get; set; }
       /// <summary>
       /// 库损日期
       /// </summary>
       public virtual DateTime GmtCreate { get; set; }

    }
}

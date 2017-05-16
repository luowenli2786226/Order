using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{
    /// <summary>
    /// 订单包装关联表
    /// </summary>
   public class OrderPackageType
    {
       public virtual int Id { get; set; }
       /// <summary>
       /// 订单号
       /// </summary>
       public virtual string OrderNo { get; set; }
       /// <summary>
       /// 是否打印
       /// </summary>
       public virtual int IsPrint { get; set; }
       /// <summary>
       /// 拣货单号
       /// </summary>
       public virtual string PickingNo { get; set; }
    }
}

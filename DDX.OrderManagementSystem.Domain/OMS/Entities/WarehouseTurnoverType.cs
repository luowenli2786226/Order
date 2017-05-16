using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain.OMS.Entities
{
  public  class WarehouseTurnoverType
    {
      /// <summary>
      /// 主键
      /// </summary>
      public virtual int TurnoverId { get; set; }
      /// <summary>
      /// 货位号
      /// </summary>
      public virtual int LocationCode { get; set; }
      /// <summary>
      /// SKUCode
      /// </summary>
      public virtual string SkuCode { get; set; }
      /// <summary>
      /// 商品上架1/下架0
      /// </summary>
      public virtual int Status { get; set; }
      /// <summary>
      /// 数量
      /// </summary>
      public virtual int Qty { get; set; }
      /// <summary>
      /// 操作人
      /// </summary>
      public virtual string CreateBy { get; set; }
      /// <summary>
      /// 操作时间
      /// </summary>
      public virtual DateTime CreateOn { get; set; }
    }
}

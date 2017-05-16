using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
   public class WarehouseSpaceImportType
    {
       /// <summary>
       /// 主键
       /// </summary>
       public virtual int Id { get; set; }
       /// <summary>
       /// 货位ID
       /// </summary>
       public virtual int SId { get; set; }
       /// <summary>
       /// 商品ID
       /// </summary>
       public virtual int PId { get; set; }
       /// <summary>
       /// 状态
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
       /// 操作日期
       /// </summary>
       public virtual DateTime UpdateOn { get; set; }
    }
}

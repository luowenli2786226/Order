using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
   public class OrdersRulersType
    {
       /// <summary>
       /// 规则ID
       /// </summary>
       public virtual int Id { get; set; }
       /// <summary>
       /// 已选条件
       /// </summary>
       public virtual string OrderChoose { get; set; }
       /// <summary>
       /// 订单自处理动作
       /// </summary>
       public virtual string OrderAction { get; set; }
       /// <summary>
       /// 发货采用的邮寄方式
       /// </summary>
       public virtual string Logictis { get; set; }
       /// <summary>
       /// 发货仓库
       /// </summary>
       public virtual string WareHouse { get; set; }
       /// <summary>
       /// 规则名称
       /// </summary>
       public virtual string RulersName { get; set; }
       /// <summary>
       /// 是否扣库存
       /// </summary>
       public virtual int IsMinusStock { get; set; }
       /// <summary>
       /// 是否正在使用中
       /// </summary>
       public virtual int IsUse { get; set; }
       /// <summary>
       /// 优先级
       /// </summary>
       public virtual int Priority { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDX.OrderManagementSystem.App
{
    public class AccountFreigheCount
    {
        /// <summary>
        /// 平台
        /// </summary>
        public virtual string Platform { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public virtual decimal Count { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public virtual decimal Amount { get; set; }

    }

    public class OrderBuyerCount
    {
      

        /// <summary>
        /// 客户名称
        /// </summary>
        public virtual String BuyerName { get; set; }

        /// <summary>
        /// 客户邮件
        /// </summary>
        public virtual String BuyerEmail { get; set; }

        /// <summary>
        /// 客户购买次数
        /// </summary>
        public virtual int BuyCount { get; set; }

        /// <summary>
        /// 客户购买金额
        /// </summary>
        public virtual double BuyAmount { get; set; }

        /// <summary>
        /// 第一次购买时间
        /// </summary>
        public virtual DateTime FristBuyOn { get; set; }

        /// <summary>
        /// 最后一次购买时间
        /// </summary>
        public virtual DateTime ListBuyOn { get; set; }

        /// <summary>
        /// 客户备注
        /// </summary>
        public virtual String Remark { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public virtual String BuyerType { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public virtual String Platform { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public virtual String Account { get; set; }

        /// <summary>
        /// 客户无理取闹
        /// </summary>
        public virtual double BuyUnreason { get; set; }
 

    }
}
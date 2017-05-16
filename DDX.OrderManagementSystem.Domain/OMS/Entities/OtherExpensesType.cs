//--------------------------------------------------------------------
// All Rights Reserved , Copyright (C)  , KeWei TECH, Ltd.
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DDX.OrderManagementSystem.Domain
{

    /// <summary>
    /// 其它费用
    /// </summary>
    public class OtherExpensesType
    {
        public virtual int Id { get; set; }
        public virtual string Platform { get; set; }
        public virtual string Account { get; set; }
        public virtual string Currency { get; set; }
        public virtual double Amount { get; set; }
        public virtual string Remarks { get; set; }
        public virtual DateTime ProcessDate { get; set; }
    }
}

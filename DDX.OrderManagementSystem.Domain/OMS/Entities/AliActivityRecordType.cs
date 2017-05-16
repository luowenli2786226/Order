namespace DDX.OrderManagementSystem.Domain
{
    using System;
    using System.Runtime.CompilerServices;

    public class AliActivityRecordType
    {
        public virtual string Account { get; set; }

        public virtual double ActivityPrice { get; set; }

        public virtual string ActivityType { get; set; }

        public virtual int ActualSales { get; set; }

        public virtual string AuditMemo { get; set; }

        public virtual double BasePrice { get; set; }

        public virtual DateTime BeginDate { get; set; }

        public virtual double CostPrice { get; set; }

        public virtual string CreateBy { get; set; }

        public virtual DateTime CreateOn { get; set; }

        public virtual DateTime EndDate { get; set; }

        public virtual string ErrorMsg { get; set; }

        public virtual int ExpectedSales { get; set; }

        public virtual double Freight { get; set; }

        public virtual int Id { get; set; }

        public virtual int IsAudit { get; set; }

        public virtual int LimitedNumber { get; set; }

        public virtual string PID { get; set; }

        public virtual string ProfitAndLoss { get; set; }

        public virtual string PUrl { get; set; }

        public virtual string Reason { get; set; }

        public virtual string Result { get; set; }

        public virtual string SKU { get; set; }

        public virtual string Status { get; set; }

        public virtual string Title { get; set; }

        public virtual double Wight { get; set; }

        public virtual double ZK { get; set; }
    }
}


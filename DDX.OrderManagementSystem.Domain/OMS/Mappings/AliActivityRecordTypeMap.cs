namespace DDX.OrderManagementSystem.Domain
{
    using FluentNHibernate.Mapping;
    using System;
    using System.Linq.Expressions;

    public class AliActivityRecordTypeMap : ClassMap<AliActivityRecordType>
    {
        public AliActivityRecordTypeMap()
        {
            base.Table("AliActivityRecord");
            base.Id((Expression<Func<AliActivityRecordType, object>>) (x => x.Id));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Title)).Length(300);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ActivityType)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.PID)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.SKU)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.PUrl)).Length(400);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.BasePrice));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ZK));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Wight));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Freight));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ActivityPrice));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Account));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.AuditMemo)).Length(0x3e8);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.CostPrice));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ExpectedSales));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ActualSales));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ProfitAndLoss)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.LimitedNumber));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Reason)).Length(0x3e8);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Result)).Length(0x7d0);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.BeginDate));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.EndDate));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.Status)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.IsAudit));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.CreateBy)).Length(100);
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.CreateOn));
            base.Map((Expression<Func<AliActivityRecordType, object>>) (x => x.ErrorMsg)).Length(300);
        }
    }
}


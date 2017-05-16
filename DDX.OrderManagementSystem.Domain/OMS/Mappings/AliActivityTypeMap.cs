namespace DDX.OrderManagementSystem.Domain
{
    using FluentNHibernate.Mapping;
    using System;
    using System.Linq.Expressions;

    public class AliActivityTypeMap : ClassMap<AliActivityType>
    {
        public AliActivityTypeMap()
        {
            base.Table("AliActivity");
            base.Id((Expression<Func<AliActivityType, object>>) (x => x.Id));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Title)).Length(200);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ActivityType)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.PID)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.SKU)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.PUrl)).Length(400);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.BasePrice));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ZK));
            base.Map((Expression<Func<AliActivityType, object>>)(x => x.TotalSale));
            base.Map((Expression<Func<AliActivityType, object>>)(x => x.Rate));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Wight));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Freight));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ActivityPrice));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Account));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.AuditMemo)).Length(0x3e8);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.CostPrice));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ExpectedSales));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ActualSales));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ProfitAndLoss)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.LimitedNumber));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Reason)).Length(0x3e8);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Result)).Length(0x7d0);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.BeginDate));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.EndDate));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.Status)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.IsAudit));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.CreateBy)).Length(100);
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.CreateOn));
            base.Map((Expression<Func<AliActivityType, object>>)(x => x.SortCode));
            base.Map((Expression<Func<AliActivityType, object>>) (x => x.ErrorMsg)).Length(300);
        }
    }
}


namespace DDX.OrderManagementSystem.Domain
{
    using FluentNHibernate.Mapping;
    using System;
    using System.Linq.Expressions;

    public class JiangChengTypeMap : ClassMap<JiangChengType>
    {
        public JiangChengTypeMap()
        {
            base.Table("JiangCheng");
            base.Id((Expression<Func<JiangChengType, object>>) (x => x.Id));
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.UserName)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.Pic)).Length(400);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.NickName)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.Content)).Length(0x7d0);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.JCBy)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.JCType)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.JCMemo)).Length(0x7d0);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.JCContent)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.JCOn));
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.CreateOn));
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.CreateBy)).Length(200);
            base.Map((Expression<Func<JiangChengType, object>>) (x => x.Area)).Length(200);
        }
    }
}


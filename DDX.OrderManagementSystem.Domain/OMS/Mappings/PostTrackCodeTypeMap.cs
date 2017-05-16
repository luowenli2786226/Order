namespace DDX.OrderManagementSystem.Domain
{
    using FluentNHibernate.Mapping;
    using System;
    using System.Linq.Expressions;

    public class PostTrackCodeTypeMap : ClassMap<PostTrackCodeType>
    {
        public PostTrackCodeTypeMap()
        {
            base.Table("PostTrackCode");
            base.Id((Expression<Func<PostTrackCodeType, object>>) (x => x.Id));
            base.Map((Expression<Func<PostTrackCodeType, object>>) (x => x.Code)).Length(20);
            base.Map((Expression<Func<PostTrackCodeType, object>>) (x => x.IsUse));
            base.Map((Expression<Func<PostTrackCodeType, object>>) (x => x.CreateOn));
            base.Map((Expression<Func<PostTrackCodeType, object>>) (x => x.CreateBy)).Length(200);

            Map(x => x.LogisticMode);
            base.Map((Expression<Func<PostTrackCodeType, object>>) (x => x.UseOn));
        }
    }
}


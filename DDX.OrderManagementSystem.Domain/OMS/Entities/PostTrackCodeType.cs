namespace DDX.OrderManagementSystem.Domain
{
    using System;
    using System.Runtime.CompilerServices;

    public class PostTrackCodeType
    {
        public virtual string Code { get; set; }

        public virtual string CreateBy { get; set; }

        public virtual DateTime CreateOn { get; set; }

        public virtual int Id { get; set; }

        public virtual int IsUse { get; set; }

        public virtual string LogisticMode { get; set; }

        public virtual DateTime UseOn { get; set; }
    }
}


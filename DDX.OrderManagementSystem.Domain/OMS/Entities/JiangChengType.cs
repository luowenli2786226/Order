namespace DDX.OrderManagementSystem.Domain
{
    using System;
    using System.Runtime.CompilerServices;

    public class JiangChengType
    {
        public virtual string Area { get; set; }

        public virtual string Content { get; set; }

        public virtual string CreateBy { get; set; }

        public virtual DateTime CreateOn { get; set; }

        public virtual int Id { get; set; }

        public virtual string JCBy { get; set; }

        public virtual string JCContent { get; set; }

        public virtual string JCMemo { get; set; }

        public virtual DateTime JCOn { get; set; }

        public virtual string JCType { get; set; }

        public virtual string NickName { get; set; }

        public virtual string Pic { get; set; }

        public virtual string UserName { get; set; }
    }
}


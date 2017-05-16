using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain
{
    public class QuestionSKUType
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        public virtual string SKU { get; set; }

        public virtual string CreateBy { get; set; }

        public virtual DateTime CreateOn { get; set; }

        public virtual string Memo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDX.OrderManagementSystem.Domain.OMS.Entities
{
    [Serializable]
    public class ConfigTempleType
    {
        public virtual long ID { get; set; }
        
        public virtual string Category { get; set; }
        public virtual decimal Price { get; set; }
        public virtual double ParamValue { get; set; }
    }
}

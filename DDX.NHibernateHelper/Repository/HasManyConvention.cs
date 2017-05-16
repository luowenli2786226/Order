using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Conventions;

namespace DDX.NHibernateHelper
{
    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IOneToManyCollectionInstance instance)
        {
            instance.Generic();
            instance.LazyLoad();
            instance.Fetch.Select();
            instance.BatchSize(10);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MagicEastern.IndexedList
{
    public static class Extensions
    {
        public static IIndexedList<T> IndexBy<T, TProperty>(this IEnumerable<T> obj, Expression<Func<T, TProperty>> expression)
        {
            IndexedList<T> ret = new IndexedList<T>(obj);
            return ret.IndexBy(expression);
        }
        
        public static List<T> LookFor<T>(this IIndexedList<T> lst, object propertyValue)
        {
            var ps = propertyValue.GetType().GetProperties().ToList();
            if (ps.Count != 1)
            {
                throw new InvalidOperationException("object propertyValue can only have one property to query the index.");
            }
            var pi = ps.Single();
            var val = pi.GetValue(propertyValue);

            var method = typeof(IIndexedList<T>).GetMethod(nameof(IIndexedList<T>.LookFor));
            var generic = method.MakeGenericMethod(pi.PropertyType);
            var ret = generic.Invoke(lst, new object[] { pi.Name, val });
            return (List<T>)ret;
        }
    }
}

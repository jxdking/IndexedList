using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.IndexedList
{
    /// <summary>
    /// The data structure is not thread safe. Concurrency should be managed by user.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexedList<T> : Index<T, T>, IIndexedList<T>
    {
        private Dictionary<string, IIndex<T>> IdxList = new Dictionary<string, IIndex<T>>();

        public IndexedList() : base(i => i, new T[0])
        {
        }

        public IndexedList(IEnumerable<T> obj) : base(i => i, obj)
        {
        }

        private PropertyInfo GetPropertyInfo<TObj, TProperty>(Expression<Func<TObj, TProperty>> propertyLambda)
        {
            Type type = typeof(TObj);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        public IIndexedList<T> IndexBy<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var pi = GetPropertyInfo(expression);
            var func = expression.Compile();
            IdxList[pi.Name] = new Index<T, TProperty>((obj) => func(obj), this);
            return this;
        }

        public HashSet<T> LookFor<TIndexed>(string propertyName, TIndexed value)
        {
            if (IdxList.TryGetValue(propertyName, out IIndex<T> val))
            {
                return ((Index<T, TIndexed>)val)[value];
            }
            throw new Exception("Index \"" + propertyName + "\" does not exists.");
        }


        public new void Add(T item)
        {
            base.Add(item);

            foreach (var idx in IdxList.Values)
            {
                idx.Add(item);
            }
        }

        /// <summary>
        /// Remove first matched object from the indexed list. Do nothing if there is no match.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public new bool Remove(T item)
        {
            var ret = base.Remove(item);
            if (ret)
            {
                foreach (var idx in IdxList.Values)
                {
                    idx.Remove(item);
                }
            }
            return ret;
        }
    }
}


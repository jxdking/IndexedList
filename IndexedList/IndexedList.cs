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
    public class IndexedList<T> : IEnumerable<T>, IIndexedList<T>
    {
        private class Index
        {
            public string Name;
            public Func<T, object> IndexBy;
            private Dictionary<Tuple<object>, List<T>> Mapping;
            public Index(string name, Func<T, object> indexBy, IEnumerable<T> data)
            {
                Name = name;
                IndexBy = indexBy;
                Mapping = data.ToLookup(indexBy).ToDictionary(i => new Tuple<object>(i.Key), i => i.ToList());
            }

            /// <summary>
            /// Add an object to this Index;
            /// </summary>
            /// <param name="obj"></param>
            public void Add(T obj)
            {
                var val = new Tuple<object>(IndexBy(obj));
                if (Mapping.TryGetValue(val, out List<T> lst))
                {
                    lst.Add(obj);
                }
                else
                {
                    Mapping[val] = new List<T> { obj };
                }
            }

            /// <summary>
            /// Remove first matched object from the index. Do nothing if there is no match.
            /// </summary>
            /// <param name="obj"></param>
            public void Remove(T obj)
            {
                var val = new Tuple<object>(IndexBy(obj));
                if (Mapping.TryGetValue(val, out List<T> lst))
                {
                    lst.Remove(obj);
                    if (lst.Count == 0)
                    {
                        Mapping.Remove(val);
                    }
                }
            }

            // Get object by index value. If no match is found, return a List with 0 member.
            public List<T> this[object idxVal]
            {
                get
                {
                    if (Mapping.TryGetValue(new Tuple<object>(idxVal), out List<T> res)) { return res; }
                    return new List<T>();
                }
            }
        }

        private Dictionary<string, Index> IdxList = new Dictionary<string, Index>();

        private List<T> _Records = new List<T>();

        /// <summary>
        /// Internal data source. Any update on this list will cause inaccurate index.
        /// Update on this list in not recommended.
        /// Call RefreshIndexes() to update the index after changing this list.
        /// </summary>
        public List<T> List
        {
            get { return _Records; }
            set { _Records = value; }
        }

        public IndexedList()
        {
            _Records = new List<T>();
        }

        public IndexedList(IEnumerable<T> obj)
        {
            _Records = obj.ToList();
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

        /// <summary>
        /// Create an index based on the Property in the expression. The index's name will be the Properties name.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IndexedList<T> IndexBy<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            IndexBy(expression, out _);
            return this;
        }

        /// <summary>
        /// Create an index with provided index's name and the function.
        /// </summary>
        /// <typeparam name="TIndexed"></typeparam>
        /// <param name="idxName"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public IndexedList<T> IndexBy<TIndexed>(string idxName, Func<T, TIndexed> func)
        {
            IndexBy(idxName, func, out _);
            return this;
        }

        private void IndexBy<TIndexed>(string idxName, Func<T, TIndexed> func, out Index idx)
        {
            IdxList[idxName] = idx = new Index(idxName, (obj) => func(obj), _Records);
        }

        private void IndexBy<TProperty>(Expression<Func<T, TProperty>> expression, out Index lookup)
        {
            var pi = GetPropertyInfo(expression);
            var func = expression.Compile();
            IndexBy(pi.Name, func, out lookup);
        }

        public void RefreshIndexes()
        {
            // re-init all the indexes
            var ks = IdxList.Keys.ToList();
            foreach (var k in ks)
            {
                var idx = IdxList[k];
                IdxList[k] = new Index(idx.Name, idx.IndexBy, _Records);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="idxName"></param>
        /// <param name="value"></param>
        /// <param name="results"></param>
        /// <returns>Return false if the index does not exist.</returns>
        private bool LookFor(string idxName, object value, out IEnumerable<T> results)
        {
            if (IdxList.TryGetValue(idxName, out Index idx))
            {
                results = idx[value];
                return true;
            }
            results = null;
            return false;
        }

        public IEnumerable<T> LookFor<TIndexed>(string idxName, TIndexed value)
        {
            if (LookFor(idxName, value, out IEnumerable<T> results))
            {
                return results;
            }
            throw new Exception("Index \"" + idxName + "\" does not exists.");
        }

        /// <summary>
        /// Look for matched items based on the expression and value.
        /// If the index has not been created, it will generate the index first and then perform the lookup.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<T> LookFor<TProperty>(Expression<Func<T, TProperty>> expression, object value)
        {
            var pi = GetPropertyInfo(expression);
            if (LookFor(pi.Name, value, out IEnumerable<T> results))
            {
                return results;
            }
            IndexBy(pi.Name, expression.Compile(), out Index idx);
            return idx[value];
        }

        public void Add(T item)
        {
            _Records.Add(item);

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
        public bool Remove(T item)
        {
            var ret = _Records.Remove(item);
            if (ret)
            {
                foreach (var idx in IdxList.Values)
                {
                    idx.Remove(item);
                }
            }
            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Records.GetEnumerator();
        }
    }

    public static class IEnumerableExtIndexedList
    {
        public static IndexedList<T> IndexBy<T, TProperty>(this IEnumerable<T> obj, Expression<Func<T, TProperty>> expression)
        {
            IndexedList<T> ret = new IndexedList<T>(obj);
            return ret.IndexBy(expression);
        }

        public static IndexedList<T> IndexBy<T>(this IEnumerable<T> obj, string idxName, Func<T, object> func)
        {
            var ret = new IndexedList<T>(obj);
            return ret.IndexBy(idxName, func);
        }
    }
}

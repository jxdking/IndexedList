using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.IndexedList
{
    /// <summary>
    /// Not thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    public class Index<T, TIndex> : IIndex<T>
    {
        private Func<T, TIndex> IndexBy;
        /// <summary>
        /// Wrap the key inside Tuple, because Dictionary does not like null keys.
        /// </summary>
        private Dictionary<Tuple<TIndex>, HashSet<T>> Mapping;
        
        public Index(Func<T, TIndex> indexBy, IEnumerable<T> data)
        {
            IndexBy = indexBy;
            Mapping = data.ToLookup(indexBy).ToDictionary(i => new Tuple<TIndex>(i.Key), i => new HashSet<T>(i));
        }

        /// <summary>
        /// Add an object to this Index;
        /// </summary>
        /// <param name="obj"></param>
        public void Add(T obj)
        {
            var val = new Tuple<TIndex>(IndexBy(obj));
            if (Mapping.TryGetValue(val, out var lst))
            {
                lst.Add(obj);
            }
            else
            {
                Mapping[val] = new HashSet<T> { obj };
            }
        }

        /// <summary>
        /// Remove first matched object from the index. Do nothing if there is no match.
        /// </summary>
        /// <param name="obj"></param>
        public bool Remove(T obj)
        {
            var val = new Tuple<TIndex>(IndexBy(obj));
            if (Mapping.TryGetValue(val, out var lst))
            {
                var ret = lst.Remove(obj);
                if (lst.Count == 0)
                {
                    Mapping.Remove(val);
                }
                return ret;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var l in Mapping.Values) {
                foreach (var i in l) {
                    yield return i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Get object by index value. If no match is found, return a set with 0 member.
        public HashSet<T> this[TIndex idxVal]
        {
            get
            {
                if (Mapping.TryGetValue(new Tuple<TIndex>(idxVal), out var res)) { return res; }
                return new HashSet<T>();
            }
        }
    }
}

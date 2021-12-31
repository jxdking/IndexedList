using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MagicEastern.IndexedList
{
    public interface IIndexedList<T>
    {
        List<T> List { get; set; }

        
        IndexedList<T> IndexBy<TIndexed>(string idxName, Func<T, TIndexed> func);
        IndexedList<T> IndexBy<TProperty>(Expression<Func<T, TProperty>> expression);
        IEnumerable<T> LookFor<TIndexed>(string idxName, TIndexed value);
        IEnumerable<T> LookFor<TProperty>(Expression<Func<T, TProperty>> expression, object value);
        
        void Add(T item);
        bool Remove(T item);
        void RefreshIndexes();
    }
}
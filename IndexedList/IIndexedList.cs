using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MagicEastern.IndexedList
{
    public interface IIndexedList<T>
    {
        IIndexedList<T> IndexBy<TProperty>(Expression<Func<T, TProperty>> expression);
        HashSet<T> LookFor<TIndexed>(string propertyName, TIndexed value);
        
        void Add(T item);
        bool Remove(T item);
    }
}
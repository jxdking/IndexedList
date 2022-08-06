using System.Collections.Generic;

namespace MagicEastern.IndexedList
{
    internal interface IIndex<T>: IEnumerable<T>
    {
        void Add(T obj);
        bool Remove(T obj);
    }
}
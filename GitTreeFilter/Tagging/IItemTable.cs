using System;

namespace GitTreeFilter.Tagging;

interface IItemTable<TKey, TValue> : IDisposable
{
    void Insert(TKey item, TValue value);

    TValue Select(TKey item);
}

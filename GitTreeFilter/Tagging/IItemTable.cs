using System;
using System.Collections.Generic;
using System.Text;

namespace GitTreeFilter.Tagging
{
    internal interface IItemTable<TKey, TValue> : IDisposable
    {
        void Insert(TKey item, TValue value);

        TValue Select(TKey item);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GitTreeFilter.Tagging
{
    internal class RenamedPathTable<TKey> : IItemTable<TKey, string>
        where TKey : class
    {
        private ConditionalWeakTable<TKey, string> conditionalWeakTable;

        public RenamedPathTable()
        {
            conditionalWeakTable = new ConditionalWeakTable<TKey, string>();
        }

        public string Select(TKey key)
        {
            this.conditionalWeakTable.TryGetValue(key, out string value);
            return value;
        }

        public void Insert(TKey key, string value)
        {
            this.conditionalWeakTable.Add(key, value);
        }

        public void Dispose()
        {
            this.conditionalWeakTable = null;
            GC.Collect();
        }
    }
}

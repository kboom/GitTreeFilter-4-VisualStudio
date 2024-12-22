using System;
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
            conditionalWeakTable.TryGetValue(key, out string value);
            return value;
        }

        public void Insert(TKey key, string value)
        {
            conditionalWeakTable.Remove(key);
            conditionalWeakTable.Add(key, value);
        }

        public void Dispose()
        {
            conditionalWeakTable = null;
            GC.Collect();
        }
    }
}

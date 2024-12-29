using System;
using System.Runtime.CompilerServices;

namespace GitTreeFilter.Tagging;

class RenamedPathTable<TKey> : IItemTable<TKey, string>
    where TKey : class
{
    #region IItemTable

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

    #endregion

    #region Constructors

    internal RenamedPathTable()
    {
        conditionalWeakTable = new ConditionalWeakTable<TKey, string>();
    }

    #endregion

    #region PrivateMembers

    private ConditionalWeakTable<TKey, string> conditionalWeakTable;

    #endregion
}

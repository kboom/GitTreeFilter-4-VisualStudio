using System;
using System.Runtime.CompilerServices;

namespace GitTreeFilter.Tagging;

class ProjectTable : IItemTable<EnvDTE.Project, string>
{
    #region IItemTable

    public string Select(EnvDTE.Project key)
    {
        _ = conditionalWeakTable.TryGetValue(key, out var value);
        return value;
    }

    public void Insert(EnvDTE.Project key, string value)
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

    internal ProjectTable() => conditionalWeakTable = new ConditionalWeakTable<EnvDTE.Project, string>();

    #endregion

    #region PrivateMembers

    private ConditionalWeakTable<EnvDTE.Project, string> conditionalWeakTable;

    #endregion
}

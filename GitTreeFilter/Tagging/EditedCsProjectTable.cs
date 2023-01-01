using System;
using System.Runtime.CompilerServices;

namespace GitTreeFilter.Tagging
{
    internal class ProjectTable : IItemTable<EnvDTE.Project, string>
    {
        private ConditionalWeakTable<EnvDTE.Project, string> conditionalWeakTable;

        public ProjectTable() => conditionalWeakTable = new ConditionalWeakTable<EnvDTE.Project, string>();

        public string Select(EnvDTE.Project key)
        {
            _ = conditionalWeakTable.TryGetValue(key, out var value);
            return value;
        }

        public void Insert(EnvDTE.Project key, string value) => conditionalWeakTable.Add(key, value);

        public void Dispose()
        {
            conditionalWeakTable = null;
            GC.Collect();
        }
    }
}

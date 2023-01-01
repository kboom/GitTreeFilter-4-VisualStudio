using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitTreeFilter.Tagging
{
    public interface IItemTagManager
    {
        void CreateTagTables();
        string GetOldFilePathFromRenamed(Project project);
        string GetOldFilePathFromRenamed(ProjectItem projectItem);
        bool IsProjEdited(Project project);
        void MarkProjAsChanged(IVsHierarchyItem vsHierarchyItem);
        void SetOldFilePathOnRenamedItem(IVsHierarchyItem vsHierarchyItem, string oldPath);
        void Reset();
    }

    internal class ItemTagManager : IItemTagManager
    {
        private RenamedPathTable<ProjectItem> renamedProjectItemTable;
        private RenamedPathTable<Project> renamedProjectTable;
        private ProjectTable editedProjectTable;

        public void CreateTagTables() => Reset();

        public void MarkProjAsChanged(IVsHierarchyItem vsHierarchyItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vsHierarchy = vsHierarchyItem.HierarchyIdentity.Hierarchy;
            _ = vsHierarchy.ParseCanonicalName(vsHierarchyItem.CanonicalName, out var itemId);
            _ = vsHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObject);
            var project = itemObject as Project;

            this.editedProjectTable.Insert(project, project.FullName);
        }

        public bool IsProjEdited(Project project) => !(editedProjectTable.Select(project) is null);

        public string GetOldFilePathFromRenamed(Project project) => renamedProjectTable.Select(project);

        public string GetOldFilePathFromRenamed(ProjectItem projectItem) => renamedProjectItemTable.Select(projectItem);

        public void SetOldFilePathOnRenamedItem(IVsHierarchyItem vsHierarchyItem, string oldPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vsHierarchy = vsHierarchyItem.HierarchyIdentity.Hierarchy;
            _ = vsHierarchy.ParseCanonicalName(vsHierarchyItem.CanonicalName, out var itemId);
            _ = vsHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObject);
            if (itemObject is ProjectItem projectItem)
            {
                renamedProjectItemTable.Insert(projectItem, oldPath);
            }

            if (itemObject is Project project)
            {
                renamedProjectTable.Insert(project, oldPath);
            }
        }

        public void Reset()
        {
            renamedProjectItemTable?.Dispose();
            renamedProjectTable?.Dispose();
            editedProjectTable?.Dispose();

            renamedProjectItemTable = new RenamedPathTable<EnvDTE.ProjectItem>();
            renamedProjectTable = new RenamedPathTable<EnvDTE.Project>();
            editedProjectTable = new ProjectTable();
        }
    }
}

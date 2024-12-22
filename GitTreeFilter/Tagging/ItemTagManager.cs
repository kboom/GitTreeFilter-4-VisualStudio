using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitTreeFilter.Tagging;

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

class ItemTagManager : IItemTagManager
{
    #region IItemTagManager

    public void CreateTagTables() => Reset();

    public void MarkProjAsChanged(IVsHierarchyItem vsHierarchyItem)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var vsHierarchy = vsHierarchyItem.HierarchyIdentity.Hierarchy;
        _ = vsHierarchy.ParseCanonicalName(vsHierarchyItem.CanonicalName, out var itemId);
        _ = vsHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObject);
        var project = itemObject as Project;

        _editedProjectTable.Insert(project, project.FullName);
    }

    public bool IsProjEdited(Project project) => !(_editedProjectTable.Select(project) is null);

    public string GetOldFilePathFromRenamed(Project project) => _renamedProjectTable.Select(project);

    public string GetOldFilePathFromRenamed(ProjectItem projectItem) => _renamedProjectItemTable.Select(projectItem);

    public void SetOldFilePathOnRenamedItem(IVsHierarchyItem vsHierarchyItem, string oldPath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var vsHierarchy = vsHierarchyItem.HierarchyIdentity.Hierarchy;
        _ = vsHierarchy.ParseCanonicalName(vsHierarchyItem.CanonicalName, out var itemId);
        _ = vsHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObject);
        if (itemObject is ProjectItem projectItem)
        {
            _renamedProjectItemTable.Insert(projectItem, oldPath);
        }

        if (itemObject is Project project)
        {
            _renamedProjectTable.Insert(project, oldPath);
        }
    }

    public void Reset()
    {
        _renamedProjectItemTable?.Dispose();
        _renamedProjectTable?.Dispose();
        _editedProjectTable?.Dispose();

        _renamedProjectItemTable = new RenamedPathTable<ProjectItem>();
        _renamedProjectTable = new RenamedPathTable<Project>();
        _editedProjectTable = new ProjectTable();
    }

    #endregion

    #region PrivateMembers

    private RenamedPathTable<ProjectItem> _renamedProjectItemTable;
    private RenamedPathTable<Project> _renamedProjectTable;
    private ProjectTable _editedProjectTable;

    #endregion
}

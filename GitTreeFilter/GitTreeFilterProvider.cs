using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;

namespace GitTreeFilter;

/// <summary>
/// Central class which provides filtering capabilities this plugin offers.
/// </summary>
[SolutionTreeFilterProvider(GitFiltersPackageGuids.guidCommonAncestorFilterCmdSetString, GitFiltersPackageGuids.CommonAncestorFilterId)]
public sealed partial class GitFilterProvider : HierarchyTreeFilterProvider
{
    [ImportingConstructor]
    public GitFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
    {
        _svcProvider = serviceProvider;
        _hierarchyCollectionProvider = hierarchyCollectionProvider;

        _ = _svcProvider.GetService(typeof(SGitFilterService));
    }

    protected override HierarchyTreeFilter CreateFilter()
    {
        var gitFilterService = _svcProvider.GetService(typeof(SGitFilterService)) as IGitFilterService;

        return new GitFileFilter(gitFilterService, _hierarchyCollectionProvider);
    }

    private readonly SVsServiceProvider _svcProvider;
    private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
}

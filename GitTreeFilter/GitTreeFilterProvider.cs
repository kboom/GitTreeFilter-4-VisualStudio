using Microsoft.VisualStudio.Shell;
using System;
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

        GitTreeFilterOutput.Log("GitFilterProvider constructed (MEF) - priming SGitFilterService");
        _ = _svcProvider.GetService(typeof(SGitFilterService));
    }

    protected override HierarchyTreeFilter CreateFilter()
    {
        GitTreeFilterOutput.Log("GitFilterProvider.CreateFilter called");

        var gitFilterService = _svcProvider.GetService(typeof(SGitFilterService)) as IGitFilterService;

        if (gitFilterService is null)
        {
            GitTreeFilterOutput.Log("GitFilterProvider.CreateFilter: SGitFilterService resolved to NULL - filter cannot work");
        }
        else
        {
            GitTreeFilterOutput.Log($"GitFilterProvider.CreateFilter: service resolved (PluginState={gitFilterService.PluginState}, SolutionRepository={(gitFilterService.SolutionRepository is null ? "null" : "present")}, ReferenceObject={(gitFilterService.ReferenceObject is null ? "null" : $"{gitFilterService.ReferenceObject.FriendlyName} ({gitFilterService.ReferenceObject.Reference?.Sha})")})");
        }

        try
        {
            var filter = new GitFileFilter(gitFilterService, _hierarchyCollectionProvider);
            GitTreeFilterOutput.Log("GitFilterProvider.CreateFilter: GitFileFilter created successfully");
            return filter;
        }
        catch (Exception ex)
        {
            GitTreeFilterOutput.Log($"GitFilterProvider.CreateFilter: FAILED to create GitFileFilter - {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    private readonly SVsServiceProvider _svcProvider;
    private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
}

﻿using GitTreeFilter.Core;
using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Tagging;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace GitTreeFilter;

public sealed partial class GitFilterProvider
{
    private sealed class GitFileFilter : HierarchyTreeFilter
    {
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
        private readonly IGitFilterService _gitFiltersService;

        public GitFileFilter(
            IGitFilterService filterService,
            IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            Assumes.Present(hierarchyCollectionProvider);
            Assumes.Present(filterService);

            _hierarchyCollectionProvider = hierarchyCollectionProvider;
            _gitFiltersService = filterService;

            Initialized += BranchDiffFilter_Initialized;
        }

        private ISolutionRepository SolutionRepository => _gitFiltersService.SolutionRepository;

        private GitFiltersObservableSet ItemSet { get; set; }

        private void BranchDiffFilter_Initialized(object sender, EventArgs e)
        {
            _gitFiltersService.GitFilterChanged += OnFilterChangedHandler;
            _gitFiltersService.IsFilterApplied = true;
        }

        public void OnFilterChangedHandler(object sender, NotifyGitFilterChangedEventArgs args) =>
            ThreadHelper.JoinableTaskFactory.RunAsync(async () => await ItemSet?.InvalidateAsync(new CancellationToken())).FileAndForget("GitTreeFilter/GitFileFilter/OnFilterChangedHandler");

        protected override void DisposeManagedResources()
        {
            _gitFiltersService.GitFilterChanged -= OnFilterChangedHandler;
            _gitFiltersService.IsFilterApplied = false;
            _gitFiltersService.ItemTagManager?.Reset();
            base.DisposeManagedResources();
        }

        protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
        {
            var root = HierarchyUtilities.FindCommonAncestor(rootItems);
            var sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(
                root.HierarchyIdentity.NestedHierarchy,
                CancellationToken
            );

            if (SolutionRepository is null)
            {
                // We are called before initialization. Return all items.
                return sourceItems;
            }

            ItemSet = await GitFiltersObservableSet.CreateGitFiltersObservableSetAsync(
                SolutionRepository,
                _gitFiltersService,
                _hierarchyCollectionProvider,
                sourceItems,
                CancellationToken);

            return ItemSet;
        }

        private static bool TryGetAbsoluteFilePath(IVsHierarchyItem hierarchyItem, out string filepath)
        {
            ThreadHelper.ThrowIfNotOnUIThread(); // get rid of that by fetching everything what was needed from the UI thread first, to avoid deadlock like here
            filepath = string.Empty;
            if (HierarchyUtilities.IsPhysicalFile(hierarchyItem.HierarchyIdentity) && IsCPPExternalDependencyFile(hierarchyItem))
            {
                return false;
            }
            else if (HierarchyUtilities.IsPhysicalFile(hierarchyItem.HierarchyIdentity))
            {
                filepath = hierarchyItem.CanonicalName;
                return true;
            }
            else if (HierarchyUtilities.IsProject(hierarchyItem.HierarchyIdentity))
            {
                var vsHierarchy = hierarchyItem.HierarchyIdentity.Hierarchy;
                _ = vsHierarchy.ParseCanonicalName(hierarchyItem.CanonicalName, out var itemId);
                _ = vsHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var itemObject);
                if (itemObject is EnvDTE.Project project)
                {
                    filepath = project.FullName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static bool IsCPPExternalDependencyFile(IVsHierarchyItem hierarchyItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            hierarchyItem.HierarchyIdentity.Hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var prjObject);
            if (prjObject is EnvDTE.Project containingProject)
            {
                if (containingProject.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}")
                {
                    if (hierarchyItem.Parent.Text.Equals("External Dependencies"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private class GitFiltersObservableSet : IReadOnlyObservableSet
        {
            private readonly ISolutionRepository _solutionRepository;
            private readonly IGitFilterService _gitFiltersHub;
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
            private readonly IReadOnlyObservableSet<IVsHierarchyItem> _allItems;

            private GitFiltersObservableSet(
                ISolutionRepository solutionRepository,
                IGitFilterService gitFiltersHub,
                IVsHierarchyItemCollectionProvider hierarchyCollectionProvider,
                IReadOnlyObservableSet<IVsHierarchyItem> allItems)
            {
                Assumes.NotNull(solutionRepository);
                Assumes.NotNull(allItems);
                Assumes.NotNull(gitFiltersHub);
                Assumes.NotNull(hierarchyCollectionProvider);

                _solutionRepository = solutionRepository;
                _allItems = allItems;
                _gitFiltersHub = gitFiltersHub;
                _hierarchyCollectionProvider = hierarchyCollectionProvider;

                _allItems.CollectionChanged += (sender, args) => CollectionChanged?.Invoke(sender, args);
            }

            public static async Task<GitFiltersObservableSet> CreateGitFiltersObservableSetAsync(
                ISolutionRepository solutionRepository,
                IGitFilterService gitFiltersHub,
                IVsHierarchyItemCollectionProvider hierarchyCollectionProvider,
                IReadOnlyObservableSet<IVsHierarchyItem> allItems,
                CancellationToken cancellationToken)
            {
                var observableSet = new GitFiltersObservableSet(solutionRepository, gitFiltersHub, hierarchyCollectionProvider, allItems);
                //await ThreadHelper.JoinableTaskFactory.RunAsyncAsVsTask(VsTaskRunContext.BackgroundThread, async ct => await observableSet.InvalidateAsync(ct));
                await observableSet.InvalidateAsync(cancellationToken);
                return observableSet;
            }

            public IFilteredHierarchyItemSet IncludedItems { get; private set; }

            public int Count => IncludedItems.Count;

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public bool Contains(object item) => IncludedItems.Contains(item);

            public IEnumerator GetEnumerator() => IncludedItems.GetEnumerator();

            public async Task<bool> InvalidateAsync(CancellationToken cancellationToken)
            {
                await TaskScheduler.Default;

                if (!TryCreateChangeset(out var changeset))
                {
                    // Displaying no items is better than displaying all as it would suggest filter doesn't work
                    IncludedItems = await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(_allItems, (_) => false, cancellationToken);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    return false;
                }

                var filter = new ChangesetFilter(changeset, _gitFiltersHub.ItemTagManager);

                IncludedItems = await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(
                    _allItems,
                    filter.CreatePredicate(),
                    cancellationToken);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return true;
            }

            private bool TryCreateChangeset(out GitChangeset changeset)
            {
                changeset = null;
                try
                {
                    changeset = _solutionRepository.Changeset;
                    return true;
                }
                catch (GitRepoNotFoundException)
                {
                    ActivityLog.LogWarning(nameof(GitFiltersObservableSet), $"Not a git repository");
                    return false;
                }
                catch (HeadNotFoundException)
                {
                    // fatal and probably won't go away, notify the user with popup
                    ActivityLog.LogWarning(nameof(GitFiltersObservableSet), $"Unable to find a HEAD in the current repository");
                    return false;
                }
                catch (GitOperationException)
                {
                    // add some less-invasive way of notifying, fall back to no items visible
                    ActivityLog.LogWarning(nameof(GitFiltersObservableSet), $"Unable to compare the selected refs");
                    return false;
                }
            }
        }

        private class ChangesetFilter
        {
            private readonly GitChangeset _changeset;
            private readonly IItemTagManager _itemTagManager;

            public ChangesetFilter(
                GitChangeset changeset,
                IItemTagManager itemTagManager)
            {
                _itemTagManager = itemTagManager;
                _changeset = changeset;
            }

            internal Predicate<IVsHierarchyItem> CreatePredicate() => (IVsHierarchyItem hierarchyItem) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (hierarchyItem == null)
                {
                    return false;
                }

                if (TryGetAbsoluteFilePath(hierarchyItem, out var absoluteFilePath))
                {
                    var gitItem = _changeset[absoluteFilePath];

                    if (gitItem != null)
                    {
                        // Mark all Project nodes found in changeset, so we only enable "Open Diff With Base" button for these project nodes.
                        if (HierarchyUtilities.IsProject(hierarchyItem.HierarchyIdentity))
                        {
                            _itemTagManager.MarkProjAsChanged(hierarchyItem);
                        }

                        // If item renamed in working branch. Tag the old path so we find the Base branch version of file using the Old Path.
                        if (!string.IsNullOrEmpty(gitItem.OldAbsoluteFilePath))
                        {
                            _itemTagManager.SetOldFilePathOnRenamedItem(hierarchyItem, gitItem.OldAbsoluteFilePath);
                        }
                        return true;
                    }
                }

                return false;
            };
        }
    }
}
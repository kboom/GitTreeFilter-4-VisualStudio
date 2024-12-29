using EnvDTE;
using GitTreeFilter.Models;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;

namespace GitTreeFilter.Commands;

class OpenDiffCommandManager
{
    internal OpenDiffCommandManager(
        DTE dte,
        IVsUIShell vsUIShell,
        IVsDifferenceService vsDifferenceService,
        IGitFilterService gitFilterService)
    {
        Assumes.Present(dte);
        Assumes.Present(vsUIShell);
        Assumes.Present(vsDifferenceService);
        Assumes.Present(gitFilterService);

        _dte = dte;
        _vsUIShell = vsUIShell;
        _vsDifferenceService = vsDifferenceService;
        _gitFilterService = gitFilterService;
    }

    internal static async Task<OpenDiffCommandManager> CreateAsync(IGitFiltersPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var dte = await package.GetServiceAsync<DTE>();
        var vsUIShell = await package.GetServiceAsync<IVsUIShell>();
        var vsDifferenceService = await package.GetServiceAsync<SVsDifferenceService, IVsDifferenceService>();

        return new OpenDiffCommandManager(
            dte,
            vsUIShell,
            vsDifferenceService,
            package.Service
        );
    }

    internal T GetSelectedObjectInSolution<T>()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var uih = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object as UIHierarchy;
        if (uih.SelectedItems is Array selectedItems && selectedItems.Length == 1)
        {
            var selectedHierarchyItem = selectedItems.GetValue(0) as UIHierarchyItem;
            var selectedObject = selectedHierarchyItem?.Object;
            if (selectedObject != null && selectedObject is T t)
            {
                return t;
            }
        }

        return default;
    }

    internal void ShowFileDiffWindow(SolutionSelectionContainer<ISolutionSelection> solutionSelectionContainer)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (string.IsNullOrEmpty(solutionSelectionContainer.FullName))
        {
            return;
        }

        if (solutionSelectionContainer.HasNoAssociatedDiffWindow(_vsUIShell))
        {
            var fileDiffProvider = new FileDiffActions(
                _vsDifferenceService,
                solutionSelectionContainer,
                _gitFilterService.ErrorPresenter,
                _gitFilterService.SolutionRepository);

            fileDiffProvider.ShowFileDiff();
        }
        else
        {
            solutionSelectionContainer.FocusAssociatedDiffWindow(_vsUIShell);
        }
    }

    private readonly DTE _dte;
    private readonly IVsUIShell _vsUIShell;
    private readonly IVsDifferenceService _vsDifferenceService;
    private readonly IGitFilterService _gitFilterService;
}
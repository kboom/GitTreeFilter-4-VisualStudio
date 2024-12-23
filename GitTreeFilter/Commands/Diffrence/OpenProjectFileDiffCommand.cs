using EnvDTE;
using GitTreeFilter.Models;
using Microsoft.VisualStudio.Shell;
using System;

namespace GitTreeFilter.Commands;

class OpenProjectFileDiffCommand : AbstractGitFiltersCommand
{
    #region AbstractGitFiltersCommand

    public override bool IsVisible
    {
        get
        {
            if (base.IsVisible)
            {
                var selectedProject = _manager.GetSelectedObjectInSolution<Project>();
                if (selectedProject != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public override void OnExecute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var selectedProject = _manager.GetSelectedObjectInSolution<Project>();
        if (selectedProject != null)
        {
            var oldPath = GitFilterService.ItemTagManager.GetOldFilePathFromRenamed(selectedProject);
            var selection = new SolutionSelectionContainer<ISolutionSelection>
            {
                Item = new SelectedProject { Native = selectedProject, OldFullPath = oldPath }
            };

            _manager.ShowFileDiffWindow(selection);
        }
    }

    #endregion

    #region Constructors
    public OpenProjectFileDiffCommand(OpenDiffCommandManager manager, IGitFilterService gitFiltersHub) : base(gitFiltersHub)
    {
        _manager = manager;
    }

    #endregion

    #region PrivateMembers

    private readonly OpenDiffCommandManager _manager;

    #endregion
}

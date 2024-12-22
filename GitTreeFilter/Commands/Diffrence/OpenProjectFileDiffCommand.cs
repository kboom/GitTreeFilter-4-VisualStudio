using System;
using EnvDTE;
using GitTreeFilter.Filters;
using GitTreeFilter.Models;
using Microsoft.VisualStudio.Shell;

namespace GitTreeFilter.Commands
{
    internal class OpenProjectFileDiffCommand : AbstractGitFiltersCommand
    {
        private readonly OpenDiffCommandManager _manager;

        public OpenProjectFileDiffCommand(OpenDiffCommandManager manager, IGitFilterService gitFiltersHub) : base(gitFiltersHub)
        {
            _manager = manager;
        }

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
    }
}

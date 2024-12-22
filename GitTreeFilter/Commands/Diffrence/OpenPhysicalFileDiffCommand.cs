using System;
using EnvDTE;
using GitTreeFilter.Models;

namespace GitTreeFilter.Commands
{
    internal class OpenPhysicalFileDiffCommand : AbstractGitFiltersCommand
    {
        private readonly OpenDiffCommandManager _manager;

        public OpenPhysicalFileDiffCommand(OpenDiffCommandManager manager, IGitFilterService gitFiltersHub) : base(gitFiltersHub)
        {
            _manager = manager;
        }

        public override void OnExecute(object sender, EventArgs e)
        {
            var selectedProjectItem = _manager.GetSelectedObjectInSolution<ProjectItem>();
            if (selectedProjectItem != null)
            {
                var oldPath = GitFilterService.ItemTagManager.GetOldFilePathFromRenamed(selectedProjectItem);
                var selection = new SolutionSelectionContainer<ISolutionSelection>
                {
                    Item = new SelectedProjectItem { 
                        Native = selectedProjectItem,
                        OldFullPath = oldPath 
                    }
                };

                _manager.ShowFileDiffWindow(selection);
            }
        }
    }
}

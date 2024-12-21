using System;
using System.Threading.Tasks;
using GitTreeFilter.Core;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace GitTreeFilter.Commands
{
    internal class ConfigureReferenceObjectCommand : AbstractGitFiltersCommand
    {
        private readonly IGitFilterSettingStore _gitFiltersSettingStore;

        public ConfigureReferenceObjectCommand(
            IGitFilterService gitFiltersHub,
            IGitFilterSettingStore gitFiltersSettingStore) : base(gitFiltersHub)
        {
            Assumes.Present(gitFiltersSettingStore);

            _gitFiltersSettingStore = gitFiltersSettingStore;
        }
        public override void OnExecute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dialog = new ConfigureReferenceObjectDialog();

            LoadBranches(dialog);
            LoadCommits(dialog);
            LoadTags(dialog);

            dialog.SetDefaultReference(_gitFiltersSettingStore.GitReference);

            bool wasSaved = dialog.ShowModal() ?? true;

            if (wasSaved)
            {
                GitFilterService.TargetReference = dialog.SelectedReference;
            }
        }

        private ISolutionRepository SolutionRepository => GitFilterService.SolutionRepository;

        private void LoadTags(ConfigureReferenceObjectDialog dialog)
        {
            var tags = SolutionRepository.GetRecentTags();
            foreach (var tag in tags)
            {
                dialog.TagListData.Add(tag);
            }

            ActivityLog.LogInformation(nameof(ConfigureReferenceObjectCommand), $"Loaded tags {tags}");
        }

        private void LoadCommits(ConfigureReferenceObjectDialog dialog)
        {
            var commits = SolutionRepository.GetRecentCommits();
            foreach (var commit in commits)
            {
                dialog.CommitListData.Add(commit);
            }

            ActivityLog.LogInformation(nameof(ConfigureReferenceObjectCommand), $"Loaded commits {commits}");
        }

        private void LoadBranches(ConfigureReferenceObjectDialog dialog)
        {
            var branches = SolutionRepository.Branches;

            foreach (var branch in branches)
            {
                dialog.BranchListData.Add(branch);
            }
        }

        public static async Task<ConfigureReferenceObjectCommand> CreateAsync(IGitFiltersPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new ConfigureReferenceObjectCommand(package.Service, package.SettingsStore);
        }
    }
}

using System;
using System.Threading.Tasks;
using GitTreeFilter.Commands;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Util;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace GitTreeFilter
{
    internal sealed class GitTreeFilterSetting : StringEnum<GitTreeFilterSetting>
    {
        public static readonly GitTreeFilterSetting TargetGitReference = Create(nameof(TargetGitReference));
        public static readonly GitTreeFilterSetting TargetGitReferenceName = Create(nameof(TargetGitReferenceName));
        public static readonly GitTreeFilterSetting TargetGitReferenceType = Create(nameof(TargetGitReferenceType));
    }

    public interface IGitFilterSettingStore
    {
        GitReference<GitCommitObject> GitReference { get; }

        void GitReferenceChanged(object sender, NotifyGitFilterChangedEventArgs e);
    }

    public class GitFilterSettingStore : IGitFilterSettingStore
    {
        private readonly WritableSettingsStore _userSettingsStore;
        private readonly Func<GitSolution> _solution;

        private string SettingStoreVault => $"{nameof(GitFilterSettingStore)}{_solution().SolutionPath}";

        public GitFilterSettingStore(WritableSettingsStore userSettingsStore, Func<GitSolution> solution)
        {
            _userSettingsStore = userSettingsStore;
            _solution = solution;
        }

        public static async Task<IGitFilterSettingStore> CreateSettingStoreAsync(IGitFiltersPackage package)
        {
            var sm = await package.GetServiceAsync<SVsSettingsManager, IVsSettingsManager>();
            var settingsManager = new ShellSettingsManager(sm);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            return new GitFilterSettingStore(userSettingsStore, () => package.Service.Solution);
        }

        private string TargetGitReferenceSha => _userSettingsStore.GetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReference.ToString(), null);

        private string TargetGitReferenceType => _userSettingsStore.GetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), null);

        private string TargetGitReferenceName => _userSettingsStore.GetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), null);

        /// <summary>
        /// Returns a git reference that is potentially obsolete.
        /// This needs rehydrating to align with the current state of the repository.
        /// </summary>
        public GitReference<GitCommitObject> GitReference
        {
            get
            {
                if (string.IsNullOrEmpty(TargetGitReferenceType)
                    || string.IsNullOrEmpty(TargetGitReferenceSha)
                    || string.IsNullOrEmpty(TargetGitReferenceName))
                {
                    return null;
                }

                var gitCommitObject = new GitCommitObject(TargetGitReferenceSha);

                // Specific values are not important here, just identity, which is the SHA and the type that holds this SHA
                switch (TargetGitReferenceType)
                {
                    case nameof(GitBranch):
                        return new GitBranch(gitCommitObject, TargetGitReferenceName);
                    case nameof(GitCommit):
                        return new GitCommit(gitCommitObject);
                    case nameof(GitTag):
                        return new GitTag(gitCommitObject, TargetGitReferenceName);
                    default:
                        // Fail gracefully
                        ActivityLog.LogWarning(nameof(ConfigureReferenceObjectCommand), $"Did not recognize target reference type {TargetGitReferenceType}");
                        return null;
                }
            }
        }

        public void GitReferenceChanged(object sender, NotifyGitFilterChangedEventArgs e)
        {
            if (!_userSettingsStore.CollectionExists(SettingStoreVault))
            {
                _userSettingsStore.CreateCollection(SettingStoreVault);
            }

            if (e?.TargetReference != null)
            {
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReference.ToString(), e.TargetReference.Reference.Sha);
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), e.TargetReference.FriendlyName);
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), e.TargetReference.GetType().Name);
            }
        }
    }
}

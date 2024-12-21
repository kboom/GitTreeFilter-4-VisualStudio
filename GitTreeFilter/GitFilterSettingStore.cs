using System;
using System.Reflection;
using System.Threading.Tasks;
using GitTreeFilter.Commands;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Util;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json.Linq;

namespace GitTreeFilter
{
    internal sealed class GitTreeFilterSetting : StringEnum<GitTreeFilterSetting>
    {
        public static readonly GitTreeFilterSetting PinToMergeHead = Create(nameof(PinToMergeHead));
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

        /// <summary>
        /// Returns a git reference that is potentially obsolete.
        /// This needs rehydrating to align with the current state of the repository.
        /// </summary>
        public GitReference<GitCommitObject> GitReference
        {
            get
            {
                UserSettingsStoreOptions options = new UserSettingsStoreOptions(_userSettingsStore, SettingStoreVault);

                if (!options.ValidateAndLogValues())
                {
                    CleanValues();
                    return default;
                }

                if (string.IsNullOrEmpty(options.TargetGitReferenceType)
                    || string.IsNullOrEmpty(options.TargetGitReferenceSha)
                    || string.IsNullOrEmpty(options.TargetGitReferenceName))
                {
                    return default;
                }

                var gitCommitObject = new GitCommitObject(options.TargetGitReferenceSha);

                // Specific values are not important here, just identity, which is the SHA and the type that holds this SHA
                switch (options.TargetGitReferenceType)
                {
                    case nameof(GitBranch):
                        return new GitBranch(gitCommitObject, options.TargetGitReferenceName, options.PinToMergeHead);
                    case nameof(GitCommit):
                        return new GitCommit(gitCommitObject);
                    case nameof(GitTag):
                        return new GitTag(gitCommitObject, options.TargetGitReferenceName, options.PinToMergeHead);
                    default:
                        ActivityLog.LogWarning(nameof(ConfigureReferenceObjectCommand), $"Did not recognize target reference type {options.TargetGitReferenceType}");
                        CleanValues();
                        return default;
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
                _userSettingsStore.SetBoolean(SettingStoreVault, GitTreeFilterSetting.PinToMergeHead.ToString(), e.TargetReference.PinToMergeHead);
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReference.ToString(), e.TargetReference.Reference.Sha);
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), e.TargetReference.FriendlyName);
                _userSettingsStore.SetString(SettingStoreVault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), e.TargetReference.GetType().Name);
            }
        }

        private void CleanValues()
        {
            _userSettingsStore.DeleteCollection(SettingStoreVault);
        }
    }

    internal class UserSettingsStoreOptions
    {
        private readonly SettingsStore _settingsStore;
        private readonly string _vault;

        public UserSettingsStoreOptions(SettingsStore settingsStore, string vault)
        {
            _settingsStore = settingsStore;
            _vault = vault;
        }

        public bool PinToMergeHead => _settingsStore.GetBoolean(_vault, GitTreeFilterSetting.PinToMergeHead, false);

        public string TargetGitReferenceSha => _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReference.ToString(), null);

        public string TargetGitReferenceType => _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), null);

        public string TargetGitReferenceName => _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), null);

        public bool ValidateAndLogValues()
        {
            bool isValid = true;
            foreach (PropertyInfo property in typeof(UserSettingsStoreOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    var value = property.GetValue(this);
                    ActivityLog.LogInformation(nameof(GitFilterSettingStore), $"{property.Name}: {value}");
                } catch
                {
                    isValid = false;
                    ActivityLog.LogWarning(nameof(GitFilterSettingStore), $"{property.Name} has invalid value, clearing it");
                }
            }
            return isValid;
        }
    }
}

using GitTreeFilter.Commands;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Util;
using Microsoft;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace GitTreeFilter;

public interface IGitFilterSettingStore
{
    GitReference<GitCommitObject> ReferenceObject { get; }

    ISessionSettings SessionSettings { get; }

    void OnGitFilterChanged(object sender, NotifyGitFilterChangedEventArgs e);
}

internal class GitFilterSettingStore : IGitFilterSettingStore
{
    internal GitFilterSettingStore(WritableSettingsStore userSettingsStore, Func<GitSolution> solution)
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
    public GitReference<GitCommitObject> ReferenceObject
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

            switch (options.TargetGitReferenceType)
            {
                case nameof(GitBranch):
                    return new GitBranch(gitCommitObject, options.TargetGitReferenceName);
                case nameof(GitCommit):
                    return new GitCommit(gitCommitObject);
                case nameof(GitTag):
                    return new GitTag(gitCommitObject, options.TargetGitReferenceName);
                default:
                    ActivityLog.LogWarning(nameof(ConfigureReferenceObjectCommand), $"Did not recognize target reference type {options.TargetGitReferenceType}");
                    CleanValues();
                    return default;
            }
        }
    }

    public ISessionSettings SessionSettings
    {
        get
        {
            UserSettingsStoreOptions options = new UserSettingsStoreOptions(_userSettingsStore, SettingStoreVault);

            if (!options.ValidateAndLogValues())
            {
                CleanValues();
                return default;
            }

            return options.SessionSettings;
        }
    }

    public void OnGitFilterChanged(object sender, NotifyGitFilterChangedEventArgs e)
    {
        if (!_userSettingsStore.CollectionExists(SettingStoreVault))
        {
            _userSettingsStore.CreateCollection(SettingStoreVault);
        }

        UserSettingsStoreOptions options = new UserSettingsStoreOptions(_userSettingsStore, SettingStoreVault);

        if (e?.SessionSettings != null)
        {
            options.SessionSettings = e?.SessionSettings;
        }

        if (e?.TargetReference != null)
        {
            options.TargetGitReferenceSha = e.TargetReference.Reference.Sha;
            options.TargetGitReferenceName = e.TargetReference.FriendlyName;
            options.TargetGitReferenceType = e.TargetReference.GetType().Name;
        }
    }

    private void CleanValues() => _userSettingsStore.DeleteCollection(SettingStoreVault);

    private string SettingStoreVault => $"{nameof(GitFilterSettingStore)}{_solution().SolutionPath}";

    private readonly WritableSettingsStore _userSettingsStore;
    private readonly Func<GitSolution> _solution;
}

internal class UserSettingsStoreOptions
{
    private readonly WritableSettingsStore _settingsStore;
    private readonly string _vault;

    public UserSettingsStoreOptions(WritableSettingsStore settingsStore, string vault)
    {
        _settingsStore = settingsStore;
        _vault = vault;
    }

    public ISessionSettings SessionSettings
    {
        get
        {
            return Core.Models.SessionSettings.Default
                .WithIncludeUnstagedChanges(_settingsStore.GetBoolean(_vault, GitTreeFilterSetting.PinToMergeHead, false));
        }
        set
        {
            Assumes.NotNull(value);
            _settingsStore.SetBoolean(_vault, GitTreeFilterSetting.PinToMergeHead, value.IncludeUnstagedChanges);
        }
    }

    public string TargetGitReferenceSha {
        get
        {
            return _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReferenceSha.ToString(), null);
        }
        set
        {
            _settingsStore.SetString(_vault, GitTreeFilterSetting.TargetGitReferenceSha.ToString(), value);
        }
    }

    public string TargetGitReferenceType
    {
        get
        {
            return _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), null);
        }
        set
        {
            _settingsStore.SetString(_vault, GitTreeFilterSetting.TargetGitReferenceType.ToString(), value);
        }
    }

    public string TargetGitReferenceName
    {
        get
        {
            return _settingsStore.GetString(_vault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), null);
        }
        set
        {
            _settingsStore.SetString(_vault, GitTreeFilterSetting.TargetGitReferenceName.ToString(), value);
        }
    }

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
    private sealed class GitTreeFilterSetting : StringEnum<GitTreeFilterSetting>
    {
        public static readonly GitTreeFilterSetting PinToMergeHead = Create(nameof(PinToMergeHead));
        public static readonly GitTreeFilterSetting TargetGitReferenceSha = Create(nameof(TargetGitReferenceSha));
        public static readonly GitTreeFilterSetting TargetGitReferenceName = Create(nameof(TargetGitReferenceName));
        public static readonly GitTreeFilterSetting TargetGitReferenceType = Create(nameof(TargetGitReferenceType));
    }
}

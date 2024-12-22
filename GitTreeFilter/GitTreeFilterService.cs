using BranchDiffer.VS.Shared;
using EnvDTE;
using GitTreeFilter.Commands;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Tagging;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitTreeFilter
{
    public interface IGitFilterService
    {
        PluginLifecycleState PluginState { get; }
        GitReference<GitCommitObject> TargetReference { get; set; }
        IGlobalSettings GlobalSettings { get; set; }
        ISessionSettings SessionSettings { get; set; }
        GitSolution Solution { get; set; }
        ISolutionRepository SolutionRepository { get; }
        IItemTagManager ItemTagManager { get; set; }
        MessagePresenter ErrorPresenter { get; }
        bool IsFilterApplied { get; set; }

        event NotifyGitFilterChangedEventHandler GitFilterChanged;

        event NotifyGitFilterLifecycleEventHandler GitFilterLifecycleEvent;
    }

    public interface SGitFilterService
    {
    }

    public class NotifyGitFilterChangedEventArgs : EventArgs
    {
        public NotifyGitFilterChangedEventArgs(
            GitReference<GitCommitObject> targetReference,
            ISessionSettings sessionSettings)
        {
            TargetReference = targetReference;
            SessionSettings = sessionSettings;
        }

        public GitReference<GitCommitObject> TargetReference { get; }

        public ISessionSettings SessionSettings { get; }
    }

    public enum PluginLifecycleState
    {
        RUNNING,
        LOADING,
        INACTIVE
    }

    public class NotifyGitFilterLifecycleEventArgs : EventArgs
    {
    }

    public delegate void NotifyGitFilterChangedEventHandler(object sender, NotifyGitFilterChangedEventArgs args);
    public delegate void NotifyGitFilterLifecycleEventHandler(object sender, NotifyGitFilterLifecycleEventArgs args);

    public class GitFilterService : SGitFilterService, IGitFilterService
    {
        public GitFilterService(GitFiltersPackage gitFiltersPackage)
        {
            Assumes.NotNull(gitFiltersPackage);
            _package = gitFiltersPackage;

            GitFilterChanged += InvalidateResources;
        }

        public GitReference<GitCommitObject> TargetReference
        {
            get => _targetReference;
            set
            {
                if (!Equals(_targetReference, value))
                {
                    _ = _solutionRepository.TryHydrate(value, out _targetReference);
                    GitFilterChanged?.Invoke(this, new NotifyGitFilterChangedEventArgs(_targetReference, default));
                }
            }
        }

        public ISessionSettings SessionSettings
        {
            get => _sessionSettings;
            set
            {
                Assumes.NotNull(value);
                if (!Equals(_sessionSettings, value))
                {
                    _sessionSettings = value;
                    GitFilterChanged?.Invoke(this, new NotifyGitFilterChangedEventArgs(default, _sessionSettings));
                }
            }
        }

        public bool IsFilterApplied
        {
            get => _isFilterApplied;
            set
            {
                _isFilterApplied = value;
                GitFilterLifecycleEvent?.Raise(this, new NotifyGitFilterLifecycleEventArgs());
            }
        }

        public PluginLifecycleState PluginState { get; private set; } = PluginLifecycleState.LOADING;

        public IGlobalSettings GlobalSettings { get; set; }

        public GitSolution Solution { get; set; }

        public IItemTagManager ItemTagManager { get; set; }

        public MessagePresenter ErrorPresenter { get; } = new MessagePresenter();

        public ISolutionRepository SolutionRepository => _solutionRepository;

        public event NotifyGitFilterChangedEventHandler GitFilterChanged;
        public event NotifyGitFilterLifecycleEventHandler GitFilterLifecycleEvent;

        internal async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _gitExt = ServiceProvider.GlobalProvider.GetService(typeof(IGitExt)) as IGitExt;
            _dte = await _package.GetServiceAsync<DTE>();

            if (_gitExt is null || _dte is null)
            {
                await FailWithMessageBoxAsync();
            }

            _gitExt.PropertyChanged += OnRepositoryChanged;
            _dte.Events.SolutionEvents.Opened += SetUp;
            _dte.Events.SolutionEvents.BeforeClosing += () =>
            {
                PluginState = PluginLifecycleState.LOADING;
                Solution = new GitSolution(string.Empty);
            };
        }

        private void SetUp() => ThreadHelper.JoinableTaskFactory.RunAsync(async () => await SetUpAsync()).FileAndForget("GitTreeFilter/GitTreeFilterService/SetUpAsync");

        private async Task SetUpAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Setting up solution information for GitFilterService"));

            if (!TryGetRootRepositoryPath(out string repositoryRootPath))
            {
                PluginState = PluginLifecycleState.INACTIVE;
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not identify GIT repository to use, deactivating the plugin for now"));
                return;
            }

            var gitSolution = new GitSolution(repositoryRootPath);

            Solution = gitSolution;
            GlobalSettings = _package.Configuration;
            ItemTagManager = new ItemTagManager();

            LiveReloadingComparisonConfig comparisonConfig = new(
                () => GlobalSettings,
                () => SessionSettings,
                () => TargetReference
            );

            _solutionRepository = SolutionRepositoryFactory.CreateSolutionRepository(Solution, comparisonConfig);

            LoadTargetGitReference();
            ItemTagManager.CreateTagTables();

            await CommandRegistrar.InitializeAsync(_package);

            PluginState = PluginLifecycleState.RUNNING;
        }

        private void OnRepositoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_gitExt.ActiveRepositories))
            {
                SetUp();
            }
        }

        private bool TryGetRootRepositoryPath(out string repositoryPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_gitExt.ActiveRepositories.Count > 0)
            {
                if (_gitExt.ActiveRepositories.Count > 1)
                {
                    string solutionPath = _dte.Solution.FullName;
                    if (!string.IsNullOrEmpty(solutionPath))
                    {
                        IGitRepositoryInfo matchingRepository = _gitExt.ActiveRepositories.FirstOrDefault(x =>
                        {
                            var repoUri = new Uri(x.RepositoryPath, UriKind.Absolute);
                            var solutionUri = new Uri(solutionPath, UriKind.Absolute);

                            return Uri.Compare(repoUri, solutionUri, UriComponents.Path, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
                        });

                        if (matchingRepository != null)
                        {
                            repositoryPath = Path.GetFullPath(matchingRepository.RepositoryPath);
                            return true;
                        }
                    }

                    Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "There are multiple repositories loaded, choosing the first one."));
                }

                IGitRepositoryInfo gitRepositoryInfo = _gitExt.ActiveRepositories.FirstOrDefault();
                repositoryPath = gitRepositoryInfo.RepositoryPath;
                return true;
            }
            else
            {
                repositoryPath = null;
                return false;
            }
        }

        private void LoadTargetGitReference()
        {
            if (TargetReference == null)
            {
                var storedGitReference = _package.SettingsStore.ReferenceObject;
                if (storedGitReference != null)
                {
                    if (_solutionRepository.TryHydrate(storedGitReference, out var reference))
                    {
                        TargetReference = reference;
                    }
                }
                if (TargetReference == null && _solutionRepository.TryGetGitBranchByName(
                    GlobalSettings.DefaultBranch,
                    out var branch))
                {
                    TargetReference = branch;
                }
            }
        }

        private void InvalidateResources(object sender, NotifyGitFilterChangedEventArgs args)
        {
            ItemTagManager?.Reset();
        }

        private async Task FailWithMessageBoxAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var uiShell = await _package.GetServiceAsync<IVsUIShell>();
            Assumes.Present(uiShell);
            var clsid = Guid.Empty;
            _ = uiShell.ShowMessageBox(
                0,
                ref clsid,
                "FirstPackage",
                "Unable to load GitTreeFilter plugin. Failed to get Visual Studio services",
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                out _);
        }

        private readonly IGitFiltersPackage _package;
        private ISolutionRepository _solutionRepository;
        private DTE _dte;
        private IGitExt _gitExt;
        private GitReference<GitCommitObject> _targetReference;
        private ISessionSettings _sessionSettings;
        private bool _isFilterApplied = false;
    }
}

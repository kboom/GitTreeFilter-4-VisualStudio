using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BranchDiffer.VS.Shared;
using EnvDTE;
using GitTreeFilter.Commands;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Tagging;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.VisualStudio.Threading;

namespace GitTreeFilter
{
    public interface IGitFilterService
    {
        PluginLifecycleState PluginState { get; }
        GitReference<GitCommitObject> TargetReference { get; set; }
        IGitFiltersConfiguration Options { get; set; }
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
        public NotifyGitFilterChangedEventArgs(GitReference<GitCommitObject> targetReference) => TargetReference = targetReference;

        public GitReference<GitCommitObject> TargetReference { get; }
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
        private readonly IGitFiltersPackage _package;
        private ISolutionRepository _solutionRepository;
        private DTE _dte;
        private GitReference<GitCommitObject> _targetReference;

        public GitFilterService(GitFiltersPackage gitFiltersPackage)
        {
            Assumes.NotNull(gitFiltersPackage);
            _package = gitFiltersPackage;
        }

        public GitReference<GitCommitObject> TargetReference
        {
            get => _targetReference;
            set
            {
                if (!Equals(_targetReference, value))
                {
                    _ = _solutionRepository.TryHydrate(value, out _targetReference);
                    _solutionRepository.GitReference = _targetReference;
                    GitFilterChanged?.Invoke(this, new NotifyGitFilterChangedEventArgs(_targetReference));
                }
            }
        }

        public PluginLifecycleState PluginState { get; private set; } = PluginLifecycleState.LOADING;

        public IGitFiltersConfiguration Options { get; set; }
        public GitSolution Solution { get; set; }
        public IItemTagManager ItemTagManager { get; set; }

        private bool _isFilterApplied = false;
        public bool IsFilterApplied
        {
            get => _isFilterApplied;
            set
            {
                _isFilterApplied = value;
                GitFilterLifecycleEvent?.Raise(this, new NotifyGitFilterLifecycleEventArgs());
            }
        }

        public MessagePresenter ErrorPresenter { get; } = new MessagePresenter();

        public ISolutionRepository SolutionRepository => _solutionRepository;

        public event NotifyGitFilterChangedEventHandler GitFilterChanged;
        public event NotifyGitFilterLifecycleEventHandler GitFilterLifecycleEvent;

        internal async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _dte = await _package.GetServiceAsync<DTE>();

            if (_dte != null)
            {
                _dte.Events.SolutionEvents.Opened += SetUp;
                _dte.Events.SolutionEvents.BeforeClosing += () =>
                {
                    PluginState = PluginLifecycleState.LOADING;
                    Solution = new GitSolution(string.Empty);
                    // also unregister commands
                };

                var solutionLoaded = await IsSolutionLoadedAsync();
                if (solutionLoaded)
                {
                    await SetUpAsync();
                }
            }
            else
            {
                await FailWithMessageBoxAsync();
            }
        }
        private async Task<bool> IsSolutionLoadedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await _package.GetServiceAsync<SVsSolution, IVsSolution>();

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out var value));

            return value is bool isSolOpen && isSolOpen;
        }

        private void SetUp()
        {
            // Don't block
            SetUpAsync();
        }

        private async Task SetUpAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Setting up solution information for GitFilterService"));

            if (!TryGetRootRepositoryPath(out string repositoryRootPath))
            {
                PluginState = PluginLifecycleState.INACTIVE;
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not identify GIT repository to use, deactivating the plugin."));
                return;
            }

            var gitSolution = new GitSolution(repositoryRootPath);

            Solution = gitSolution;
            Options = _package.Configuration;
            ItemTagManager = new ItemTagManager();

            _solutionRepository = SolutionRepositoryFactory.CreateSolutionRepository(Solution, Options.ToComparisonConfig());

            ReadDefaults();

            await CommandRegistrar.InitializeAsync(_package);

            PluginState = PluginLifecycleState.RUNNING;
        }

        private bool TryGetRootRepositoryPath(out string repositoryPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var gitExt = (IGitExt)ServiceProvider.GlobalProvider.GetService(typeof(IGitExt));
            if (gitExt != null && gitExt.ActiveRepositories.Count > 0)
            {
                if (gitExt.ActiveRepositories.Count > 1)
                {
                    string solutionPath = _dte.Solution.FullName;
                    if (!string.IsNullOrEmpty(solutionPath))
                    {
                        IGitRepositoryInfo matchingRepository = gitExt.ActiveRepositories.FirstOrDefault(x =>
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

                IGitRepositoryInfo gitRepositoryInfo = gitExt.ActiveRepositories.FirstOrDefault();
                repositoryPath = gitRepositoryInfo.RepositoryPath;
                return true;
            }
            else
            {
                repositoryPath = null;
                return false;
            }
        }

        private void ReadDefaults()
        {
            LoadTargetGitReference();
            ItemTagManager.CreateTagTables();
        }

        private void LoadTargetGitReference()
        {
            if (TargetReference == null)
            {
                var storedGitReference = _package.SettingsStore.GitReference;
                if (storedGitReference != null)
                {
                    if (_solutionRepository.TryHydrate(storedGitReference, out var reference))
                    {
                        TargetReference = reference;
                    }
                }
                if (TargetReference == null && _solutionRepository.TryGetGitBranchByName(
                    Options.DefaultBranch,
                    out var branch))
                {
                    TargetReference = branch;
                }
            }
            _solutionRepository.GitReference = TargetReference;
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
    }
}

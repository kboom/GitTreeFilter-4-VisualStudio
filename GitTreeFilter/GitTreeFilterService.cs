using System;
using System.Diagnostics;
using System.Globalization;
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
        LOADING
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
                if(!Equals(_targetReference, value))
                {
                    _targetReference = _solutionRepository.LoadFromRepository(value);
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
                _dte.Events.SolutionEvents.BeforeClosing += () => {
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
            var absoluteSolutionPath = _dte.Solution.FullName;
            var solutionDirectory = System.IO.Path.GetDirectoryName(absoluteSolutionPath);
            var gitSolution = new GitSolution(solutionDirectory);

            Solution = gitSolution;
            Options = _package.Configuration;
            ItemTagManager = new ItemTagManager();

            _solutionRepository = SolutionRepositoryFactory.CreateSolutionRepository(Solution, Options.ToComparisonConfig());

            ReadDefaults();

            await CommandRegistrar.InitializeAsync(_package);

            PluginState = PluginLifecycleState.RUNNING;
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
                    TargetReference = _solutionRepository.LoadFromRepository(storedGitReference);
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

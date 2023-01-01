using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BranchDiffer.VS.Shared;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitTreeFilter
{
    public interface IGitFiltersPackage
    {
        IGitFilterService Service { get; }

        IGitFiltersConfiguration Configuration { get; }

        IGitFilterSettingStore SettingsStore { get; }

        Task<T> GetServiceAsync<T>();

        Task<T> GetServiceAsync<S, T>();
    }
    
    // todo store and load setting!
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(GitFiltersPackageGuids.PackageGuidString)]
    [ProvideService((typeof(SGitFilterService)), IsAsyncQueryable = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(GitFiltersConfiguration),
    "Git Tree Filters", "Git Tree Filters Options", 0, 0, true)]
    public sealed class GitFiltersPackage : AsyncPackage, IGitFiltersPackage
    {
        public IGitFilterService Service { get; private set; }

        public IGitFiltersConfiguration Configuration { get; private set; }

        public IGitFilterSettingStore SettingsStore { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            Configuration = (GitFiltersConfiguration)GetDialogPage(typeof(GitFiltersConfiguration));
            SettingsStore = await GitFilterSettingStore.CreateSettingStoreAsync(this);
            AddService(typeof(SGitFilterService), CreateGitFilterServiceAsync, true);
        }

        private void WireComponents() => Service.GitFilterChanged += SettingsStore.GitReferenceChanged;

        public async Task<T> GetServiceAsync<T>() => (T)await GetServiceAsync(typeof(T));

        public async Task<T> GetServiceAsync<S, T>() => (T) await GetServiceAsync(typeof(S));

        public async Task<object> CreateGitFilterServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            var service = new GitFilterService(this);
            Service = service;
            await service.InitializeAsync(cancellationToken);
            WireComponents();
            return service;
        }

    }
}

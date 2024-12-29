using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace GitTreeFilter;

interface IGitFiltersPackage
{
    IGitFilterService Service { get; }

    IGlobalSettings Configuration { get; }

    IGitFilterSettingStore SettingsStore { get; }

    Task<T> GetServiceAsync<T>();

    Task<T> GetServiceAsync<S, T>();
}

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(GitFiltersPackageGuids.PackageGuidString)]
[ProvideService((typeof(SGitFilterService)), IsAsyncQueryable = true)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideOptionPage(typeof(GlobalSettings), "Git Tree Filters", "Git Tree Filters Options", 0, 0, true)]
public sealed class GitFiltersPackage : AsyncPackage, IGitFiltersPackage
{
    #region AsyncPackage
    public async Task<T> GetServiceAsync<T>() => (T)await GetServiceAsync(typeof(T));

    public async Task<T> GetServiceAsync<S, T>() => (T)await GetServiceAsync(typeof(S));

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);
        Configuration = (GlobalSettings)GetDialogPage(typeof(GlobalSettings));
        SettingsStore = await GitFilterSettingStore.CreateSettingStoreAsync(this);
        AddService(typeof(SGitFilterService), CreateGitFilterServiceAsync, true);
    }

    #endregion

    #region IGitFiltersPackage

    public IGitFilterService Service { get; private set; }

    public IGlobalSettings Configuration { get; private set; }

    public IGitFilterSettingStore SettingsStore { get; private set; }

    #endregion

    #region PrivateMembers

    private void WireComponents() => Service.GitFilterChanged += SettingsStore.OnGitFilterChanged;

    private async Task<object> CreateGitFilterServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
    {
        var service = new GitFilterService(this);
        Service = service;
        await service.InitializeAsync(cancellationToken);
        WireComponents();
        return service;
    }

    #endregion
}

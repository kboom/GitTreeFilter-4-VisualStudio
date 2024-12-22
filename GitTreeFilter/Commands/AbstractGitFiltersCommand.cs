using Microsoft;
using System;

namespace GitTreeFilter.Commands;

internal abstract class AbstractGitFiltersCommand : GitFiltersCommand
{
    public event CommandVisibilityChangedEventHandler VisibilityChanged;

    protected IGitFilterService GitFilterService { get; private set; }

    public AbstractGitFiltersCommand(IGitFilterService gitFilterService)
    {
        Assumes.Present(gitFilterService);
        GitFilterService = gitFilterService;

        GitFilterService.GitFilterLifecycleEvent += OnLifecycleEvent;
    }

    private void OnLifecycleEvent(object sender, NotifyGitFilterLifecycleEventArgs args) =>
            VisibilityChanged?.Raise(this, new NotifyCommandVisibilityChangedEventArgs());
    
    public abstract void OnExecute(object sender, EventArgs e);

    public virtual bool IsVisible => GitFilterService.IsFilterApplied;
}

using GitTreeFilter.Core.Models;
using System;

namespace GitTreeFilter;

public delegate void NotifyGitFilterChangedEventHandler(object sender, NotifyGitFilterChangedEventArgs args);

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

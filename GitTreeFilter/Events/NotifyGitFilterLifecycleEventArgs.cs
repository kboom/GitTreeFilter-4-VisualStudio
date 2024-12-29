using System;

namespace GitTreeFilter;

public delegate void NotifyGitFilterLifecycleEventHandler(object sender, NotifyGitFilterLifecycleEventArgs args);

public class NotifyGitFilterLifecycleEventArgs : EventArgs
{
}

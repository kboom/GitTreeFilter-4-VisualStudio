using System;

namespace GitTreeFilter.Commands
{
    internal interface GitFiltersCommand
    {
        void OnExecute(object sender, EventArgs e);

        bool IsVisible { get; }

        event CommandVisibilityChangedEventHandler VisibilityChanged;
    }

    public delegate void CommandVisibilityChangedEventHandler(object sender, NotifyCommandVisibilityChangedEventArgs args);

    public class NotifyCommandVisibilityChangedEventArgs : EventArgs
    {
    }
}

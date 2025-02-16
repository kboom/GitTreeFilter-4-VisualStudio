using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

public static class GitTreeFilterOutput
{
    private static IVsOutputWindowPane _outputPane;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (_outputPane == null)
        {
            var outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow != null)
            {
                Guid customPaneGuid = Guid.NewGuid();
                outputWindow.CreatePane(ref customPaneGuid, nameof(GitTreeFilter), 1, 1);
                outputWindow.GetPane(ref customPaneGuid, out _outputPane);
            }
        }
    }

    public static void WriteLine(string message)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_outputPane != null)
        {
            _outputPane.OutputStringThreadSafe(message + Environment.NewLine);
        }
    }
}

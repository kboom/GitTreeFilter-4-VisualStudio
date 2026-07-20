using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Globalization;
using System.Threading;

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
        // OutputStringThreadSafe is safe to call from any thread, so we do not
        // require the UI thread here. Diagnostic logging must work from the
        // background threads that drive filter creation and changeset building.
        _outputPane?.OutputStringThreadSafe(message + Environment.NewLine);
    }

    /// <summary>
    /// Writes a diagnostic line prefixed with a timestamp and the current managed
    /// thread id. Safe to call from any thread. Use for tracing the activation and
    /// filter/changeset flow so the sequence and threading of events is visible.
    /// </summary>
    public static void Log(string message)
    {
        var line = string.Format(
            CultureInfo.InvariantCulture,
            "[{0:HH:mm:ss.fff} T{1}] {2}",
            DateTime.Now,
            Thread.CurrentThread.ManagedThreadId,
            message);

        WriteLine(line);
    }
}

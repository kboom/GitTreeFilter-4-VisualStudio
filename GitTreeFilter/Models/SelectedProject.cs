using EnvDTE;

namespace GitTreeFilter.Models;

class SelectedProject : ISolutionSelection
{
    #region ISolutionSelection

    public string FullPath
    {
        get
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return Native.FullName;
        }
    }

    public string OldFullPath { get; set; }

    public string Kind
    {
        get
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return Native.Kind;
        }
    }

    public Document Document => null;

    #endregion

    internal Project Native { get; set; }
}

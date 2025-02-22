﻿using EnvDTE;

namespace GitTreeFilter.Models;

public class SelectedProjectItem : ISolutionSelection
{
    #region ISolutionSelection

    public Document Document
    {
        get 
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return this.Native.Document;
        }
    }

    public string FullPath
    {
        get
        {
            // If Properties is null, use FileNames[] array to get physical path of the ProjectItem. No idea why it is 1-based index.
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return Native.Properties is null ? Native.FileNames[1] : Native.Properties.Item("FullPath")?.Value.ToString();
        }
    }

    public string OldFullPath { get; set; }

    public string Kind
    {
        get
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return this.Native.Kind;
        }
    }

    #endregion

    internal ProjectItem Native { get; set; }
}

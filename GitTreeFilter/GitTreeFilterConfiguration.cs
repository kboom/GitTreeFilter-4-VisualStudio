using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace GitTreeFilter;

/// <summary>
/// Global configuration for the Git Tree Filter plugin.
/// Managed from within the Visual Studio settings pages.
/// </summary>
public interface IGlobalSettings {
    string DefaultBranch { get; }

    bool OriginRefsOnly { get; }
}

public class GlobalSettings : DialogPage, IGlobalSettings
{
    private const string c_category = "Git Tree Filter";

    [Category(c_category)]
    [DisplayName("Default branch to compare to")]
    [Description("The default branch for which a tree to compare to will be taken.")]
    public string DefaultBranch { get; set; } = "master";

    [Category(c_category)]
    [DisplayName("Display only origin references")]
    [Description("In case your branch pulled changes from origin but you are comparing to local master you will see unrelated changes. This prevents you from comparing to local branches.")]
    public bool OriginRefsOnly { get; set; } = false;
}

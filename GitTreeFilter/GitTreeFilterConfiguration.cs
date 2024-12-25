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
    public string DefaultBranch { get; set; } = "origin/master";

    [Category(c_category)]
    [DisplayName("Display only origin references")]
    [Description("If your branch has pulled changes from the origin that are not present in the local branch you're comparing to, unrelated changes will appear. This makes it difficult to compare with local branches. Setting this option to 'true' (default) is recommended.")]
    public bool OriginRefsOnly { get; set; } = true;
}

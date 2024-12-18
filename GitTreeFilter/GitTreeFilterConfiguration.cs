using System.ComponentModel;
using GitTreeFilter.Core;
using Microsoft.VisualStudio.Shell;

namespace BranchDiffer.VS.Shared
{
    public interface IGitFiltersConfiguration {
        string DefaultBranch { get; }

        bool OriginRefsOnly { get; }
    }

    public static class GitFiltersConfigurationExt
    {
        public static IComparisonConfig ToComparisonConfig(this IGitFiltersConfiguration globalConfig) => new ConfigurationComparisonConfig(globalConfig);
    }

    class ConfigurationComparisonConfig : IComparisonConfig
    {
        private readonly IGitFiltersConfiguration _config;

        public ConfigurationComparisonConfig(IGitFiltersConfiguration globalConfig) => _config = globalConfig;

        public bool OriginRefsOnly => _config.OriginRefsOnly;
    }

    public class GitFiltersConfiguration : DialogPage, IGitFiltersConfiguration
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
}

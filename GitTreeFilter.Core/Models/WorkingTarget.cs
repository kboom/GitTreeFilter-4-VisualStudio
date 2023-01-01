using GitTreeFilter.Core.Models;
using LibGit2Sharp;

namespace GitTreeFilter.Core
{
    /// <summary>
    /// This class should only be used within the disposable context
    /// </summary>
    internal class WorkingTarget
    {
        /// <summary>
        /// The working branch of Git Repo
        /// </summary>
        public Branch WorkingBranch { get; set; }

        /// <summary>
        /// The branch against which a diff will be run.
        /// </summary>
        public Commit TargetCommit { get; set; }

    }
}

using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Extensions
{
    public static class GitObjectExtensions
    {
        /// <summary>
        /// Converts a commit to its basic form which is stored in the settings.
        /// </summary>
        /// <param name="gitCommit">The commit to dehydrate</param>
        /// <returns>Dehydrated commit described only by the commit SHA</returns>
        public static GitCommit Dehydrate(this GitCommit gitCommit) => new(new GitCommitObject(gitCommit.Reference.Sha));

        /// <summary>
        /// Converts a branch to its basic form which is stored in the settings.
        /// </summary>
        /// <param name="gitBranch">The branch to dehydrate</param>
        /// <returns>Dehydrated branch described only by the commit SHA and the name</returns>
        public static GitBranch Dehydrate(this GitBranch gitBranch) => new(new GitCommitObject(gitBranch.Reference.Sha), gitBranch.FriendlyName, gitBranch.PinToMergeHead);

        /// <summary>
        /// Converts a tag to its basic form which is stored in the settings.
        /// </summary>
        /// <param name="gitTag">The tag to dehydrate</param>
        /// <returns>Dehydrated tag described only by the commit SHA and name</returns>
        public static GitTag Dehydrate(this GitTag gitTag) => new(new GitCommitObject(gitTag.Reference.Sha), gitTag.Reference.Sha, gitTag.PinToMergeHead);
    }
}

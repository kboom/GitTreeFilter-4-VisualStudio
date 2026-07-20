using System;
using GitTreeFilter.Core.Tests.Test.Repositories;
using LibGit2Sharp;

namespace GitTreeFilter.Core.Tests.Test
{
    /// <summary>
    /// Helpers to temporarily manipulate the on-disk state of a test repository
    /// working directory so that scenarios such as a detached HEAD can be exercised.
    /// </summary>
    internal static class RepositoryState
    {
        /// <summary>
        /// Runs <paramref name="action"/> while the working directory of
        /// <paramref name="testRepository"/> is in a detached HEAD state.
        /// HEAD is detached at the current branch tip, so no working-tree files
        /// change. The originally checked out branch is always restored afterwards.
        /// </summary>
        public static void WithDetachedHead(ITestRepository testRepository, Action action)
        {
            var path = testRepository.Solution.SolutionPath;
            using var repository = new Repository(path);

            if (repository.Info.IsHeadDetached)
            {
                action();
                return;
            }

            var originalBranch = repository.Head.FriendlyName;
            var tip = repository.Head.Tip;

            try
            {
                // Checking out a commit (rather than a branch) detaches HEAD.
                Commands.Checkout(repository, tip);
                action();
            }
            finally
            {
                Commands.Checkout(repository, repository.Branches[originalBranch]);
            }
        }
    }
}

using GitTreeFilter.Core.Models;
using LibGit2Sharp;
using System;
using System.Linq;

namespace GitTreeFilter.Core
{
    internal static class RepositoryExtensions
    {
        public static Commit GetTargetCommit(this IRepository repository, GitReference<GitCommitObject> target) =>
            repository.Lookup<Commit>(new ObjectId(target.Reference.Sha));

        public static bool TryHydrate(
            this IRepository repository,
            GitReference<GitCommitObject> target,
            out GitReference<GitCommitObject> hydratedReference,
            IComparisonConfig comparisonConfig)
        {
            hydratedReference = null;
            switch (target)
            {
                case GitBranch gitBranch:
                    hydratedReference = repository.LoadGitBranch(gitBranch, comparisonConfig);
                    break;
                case GitTag gitTag:
                    hydratedReference = repository.LoadGitTag(gitTag);
                    break;
                case GitCommit gitCommit:
                    hydratedReference = repository.LoadGitCommit(gitCommit);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return hydratedReference != null;
        }

        /// <summary>
        /// Fast-forward to the up-to-date commit (history can be rewritten in some cases)
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="gitTag"></param>
        /// <returns></returns>
        public static GitReference<GitCommitObject> LoadGitCommit(this IRepository repository, GitCommit gitCommit)
        {
            var commit = repository.Lookup<Commit>(new ObjectId(gitCommit.Reference.Sha));
            return commit?.ToGitCommitObject()?.ToGitCommit();
        }

        /// <summary>
        /// Fast-forward to the newest available commit for a tag (should be the same but not guaranteed if deleted and recreated).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="gitTag"></param>
        /// <returns></returns>
        public static GitReference<GitCommitObject> LoadGitTag(this IRepository repository, GitTag gitTag)
        {
            var tag = !string.IsNullOrEmpty(gitTag.FriendlyName) ? repository.Tags[gitTag.FriendlyName] : null;

            if (tag == null)
            {
                var objectId = new ObjectId(gitTag.Reference.Sha);
                var commit = repository.Lookup<Commit>(objectId);
                return commit?.ToGitCommitObject()?.ToGitCommit();
            }
            else
            {
                var commit = repository.Lookup<Commit>(tag.Target.Id);
                return new GitTag(commit.ToGitCommitObject(), tag.FriendlyName);
            }
        }

        /// <summary>
        /// Fast-forward to the newest available commit on the branch or if no branch available anymore use the commit reference instead.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="gitBranch"></param>
        /// <returns></returns>
        public static GitReference<GitCommitObject> LoadGitBranch(this IRepository repository, GitBranch gitBranch, IComparisonConfig config)
        {
            try
            {
                var branch = !string.IsNullOrEmpty(gitBranch.FriendlyName) ? repository.Branches[gitBranch.FriendlyName] : null;
                Commit commitToUse = branch.Tip;
                return new GitBranch(commitToUse.ToGitCommitObject(), branch.FriendlyName);
            } catch
            {
                // Branch removed, fall back to commit
                var objectId = new ObjectId(gitBranch.Reference.Sha);
                var commit = repository.Lookup<Commit>(objectId);
                return commit?.ToGitCommitObject()?.ToGitCommit();
            }
        }

        public static GitCommit ToGitCommit(this Commit x) => x.ToGitCommitObject().ToGitCommit();

        public static GitCommitObject ToGitCommitObject(this Commit x) => new GitCommitObject(x.Sha, x.MessageShort);

        public static GitCommit ToGitCommit(this GitCommitObject x) => new GitCommit(x);

        public static GitTag ToGitTag(this GitCommitObject x, string name) => new GitTag(x, name);

    }
}

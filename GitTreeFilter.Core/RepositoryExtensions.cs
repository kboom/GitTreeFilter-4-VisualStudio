using System;
using GitTreeFilter.Core.Models;
using LibGit2Sharp;

namespace GitTreeFilter.Core
{
    internal static class RepositoryExtensions
    {
        public static Commit GetTargetCommit(this IRepository repository, GitReference<GitCommitObject> target) =>
            repository.Lookup<Commit>(new ObjectId(target.Reference.Sha));

        public static GitReference<GitCommitObject> LoadFromRepository(this IRepository repository, GitReference<GitCommitObject> target, IComparisonConfig comparisonConfig)
        {
            switch (target)
            {
                case GitBranch gitBranch:
                    return repository.LoadGitBranch(gitBranch, comparisonConfig);
                case GitTag gitTag:
                    return repository.LoadGitTag(gitTag);
                case GitCommit gitCommit:
                    return repository.LoadGitCommit(gitCommit);
                default:
                    throw new NotImplementedException();
            }
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
            var branch = !string.IsNullOrEmpty(gitBranch.FriendlyName) ? repository.Branches[gitBranch.FriendlyName] : null;

            if (string.IsNullOrEmpty(gitBranch.FriendlyName))
            {
                var objectId = new ObjectId(gitBranch.Reference.Sha);
                var commit = repository.Lookup<Commit>(objectId);
                return commit?.ToGitCommitObject()?.ToGitCommit();
            }

            return new GitBranch(branch.Tip.ToGitCommitObject(), branch.FriendlyName);
        }

        public static GitCommit ToGitCommit(this Commit x) => x.ToGitCommitObject().ToGitCommit();

        public static GitCommitObject ToGitCommitObject(this Commit x) => new GitCommitObject(x.Sha, x.MessageShort);

        public static GitCommit ToGitCommit(this GitCommitObject x) => new GitCommit(x);

        public static GitTag ToGitTag(this GitCommitObject x, string name) => new GitTag(x, name);

    }
}

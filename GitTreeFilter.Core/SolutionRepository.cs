using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitTreeFilter.Core
{
    public interface ISolutionRepository
    {
        /// <summary>
        /// The solution this repository is configured for.
        /// </summary>
        GitSolution GitSolution { get; }

        /// <summary>
        /// Configuration of the solution repository.
        /// This will affect the behavior of some methods.
        /// </summary>
        IComparisonConfig ComparisonConfig { get; }

        /// <summary>
        /// All branches that are defined for the repository <see cref="GitSolution"/>.
        /// </summary>
        IEnumerable<GitBranch> Branches { get; }

        /// <summary>
        /// Generates the <see cref="GitChangeset"/> between the current working directory <see cref="GitSolution"/>
        /// and a Git Worktree <see cref="GitReference"/>.
        /// </summary>
        GitChangeset Changeset { get; }

        /// <summary>
        /// Attempts to read an element in the repository at <paramref name="path"/> in the current repository working directory.
        /// Returns <see langword="true"/> and sets an element <paramref name="item"/> if available,
        /// or <see langword="false"/> with element <paramref name="item"/> set to <see langword="null"/> otherwise.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryReadItem(string path, out GitItem item);

        /// <summary>
        /// Gets the <paramref name="number"/> of commits which are reachable from current repository HEAD,
        /// sorted by topology and time.
        /// </summary>
        /// <param name="number">The maximum number of commits to return, or 50 by default</param>
        /// <returns></returns>
        IEnumerable<GitCommit> GetRecentCommits(int number = 50);

        /// <summary>
        /// Get the <paramref name="number"/> of tags by the order of creation date.
        /// </summary>
        /// <param name="number">The maximum number of tags to return, or 50 by default</param>
        /// <returns>Tags</returns>
        IEnumerable<GitTag> GetRecentTags(int number = 50);

        /// <summary>
        /// Attempts to load a reference from the repository, restoring its full details.
        /// If it is not possible to load the original refrence, a git commit will be returned.
        /// Returns false only in the case the commit no longer exists in the repository.
        /// </summary>
        /// <param name="gitReference">Dehydrated reference to be hydrated</param>
        /// <returns>Hydrated reference of original type or a hydrated commit, if original object no longer resolvable</returns>
        bool TryHydrate(GitReference<GitCommitObject> gitReference, out GitReference<GitCommitObject> hydratedReference);

        /// <summary>
        /// Attempts to load a branch by its name from repository.
        /// Returns false if no branch with that name exists.
        /// </summary>
        /// <param name="branchName">The name of the branch to resolve</param>
        /// <param name="branch">The branch resolved or <see langword="null"/> if branch does not exist</param>
        /// <returns></returns>
        bool TryGetGitBranchByName(string branchName, out GitBranch branch);
    }

    internal class SolutionRepository : ISolutionRepository
    {
        public GitSolution GitSolution { get; }

        public IComparisonConfig ComparisonConfig { get; }

        private readonly GitRepositoryFactory _repositoryFactory;

        public IEnumerable<GitBranch> Branches
        {
            get
            {
                using var repository = _repositoryFactory.Create(GitSolution);
                return repository.Branches
                    .Where(p => !ComparisonConfig.OriginRefsOnly || p.IsRemote)
                    .Select(x => new GitBranch(x.Tip.ToGitCommitObject(), x.FriendlyName))
                    .ToList();
            }
        }

        public GitChangeset Changeset
        {
            get
            {
                using var repository = _repositoryFactory.Create(GitSolution);

                AssertValidReference(repository, ComparisonConfig.ReferenceObject);

                var changeset = CollectTreeChanges(repository);

                // this works, but is quite slow
                var repositoryStatus = repository.RetrieveStatus(new StatusOptions()
                {
                    Show = StatusShowOption.IndexAndWorkDir,
                    IncludeIgnored = false,
                    ExcludeSubmodules = true,
                    RecurseUntrackedDirs = false,
                    DisablePathSpecMatch = true,
                    IncludeUnaltered = false,
                    IncludeUntracked = true,

                    DetectRenamesInIndex = false,
                    DetectRenamesInWorkDir = false

                });
                var filesToAdd = GetExtraFilesToAdd(repositoryStatus).ToList();

                // For the untracked changes, we should also compare them to history commit since otherwise there will be no "compare diff" capability
                var untrackedChanges = filesToAdd.Select(x => CreateDiffedObject(repository, x)).ToHashSet();

                var filesToRemove = GetExtraFilesToRemove(repositoryStatus).ToHashSet();

                changeset.RemoveWhere(x => filesToRemove.Contains(x.AbsoluteFilePath));
                changeset.UnionWith(untrackedChanges);

                return new GitChangeset(changeset);
            }
        }

        public IEnumerable<GitCommit> GetRecentCommits(int number = 50)
        {
            using var repository = _repositoryFactory.Create(GitSolution);
            return repository.Commits
                .QueryBy(new CommitFilter()
                {
                    FirstParentOnly = true,
                    SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time,
                    IncludeReachableFrom = repository.Head.Tip
                })
                .Take(number)
                .Select(x => x.ToGitCommit())
                .ToList();
        }

        public IEnumerable<GitTag> GetRecentTags(int number = 50)
        {
            using var repository = _repositoryFactory.Create(GitSolution);
            return repository.Tags
                 .Where(x => !x.PeeledTarget.IsMissing)
                 .Take(number)
                 .Select(x =>
                 {
                     try
                     {
                         var commit = repository.Lookup<Commit>(x.Target.Id);
                         var gitCommit = RepositoryExtensions.ToGitCommitObject(commit);
                         return RepositoryExtensions.ToGitTag(gitCommit, x.FriendlyName);
                     }
                     catch
                     {
                         return null;
                     }
                 })
                 .Where(x => x != null)
                 .ToList();
        }

        public bool TryGetGitBranchByName(string branchName, out GitBranch branch)
        {
            using var repository = _repositoryFactory.Create(GitSolution);
            branch = null;
            if (string.IsNullOrEmpty(branchName))
            {
                return false;
            }

            var branchObj = repository.Branches[branchName];
            if (branchObj == null)
            {
                return false;
            }

            var commit = branchObj.Tip;
            branch = new GitBranch(RepositoryExtensions.ToGitCommitObject(commit), branchName);
            return true;
        }

        public bool TryReadItem(string path, out GitItem item)
        {
            item = null;
            using var repository = _repositoryFactory.Create(GitSolution);
            var compareOptions = new CompareOptions
            {
                Algorithm = DiffAlgorithm.Minimal,
                IncludeUnmodified = false,
            };

            var targetCommit = RepositoryExtensions.GetTargetCommit(repository, ComparisonConfig.ReferenceObject);

            var branchDiffResult = repository.Diff.Compare<TreeChanges>(
               targetCommit.Tree,
                DiffTargets.WorkingDirectory, Enumerable.Repeat(path, 1));

            var changes = GetFileDiffViewableChanges(branchDiffResult);
            item = changes.Select(x => CreateDiffedObject(repository, x)).FirstOrDefault();

            if (item == default)
            {
                return false;
            }
            return true;
        }

        public bool TryHydrate(GitReference<GitCommitObject> gitReference, out GitReference<GitCommitObject> hydratedReference)
        {
            using var repository = _repositoryFactory.Create(GitSolution);
            return RepositoryExtensions.TryHydrate(repository, gitReference, out hydratedReference, ComparisonConfig);
        }

        internal SolutionRepository(
            GitSolution gitSolution,
            IComparisonConfig comparisionConfig,
            GitRepositoryFactory repositoryFactory)
        {
            GitSolution = gitSolution;
            ComparisonConfig = comparisionConfig;
            _repositoryFactory = repositoryFactory;
        }

        private GitItem CreateDiffedObject(
            IRepository gitRepo,
            string itemPath)
        {
            var itemPathWithCorrectSeparator = itemPath.Replace('/', Path.DirectorySeparatorChar);
            var repoPathWithCorrectSeperator = gitRepo.Info.WorkingDirectory.Replace('/', Path.DirectorySeparatorChar);

            var absoluteFilePath = repoPathWithCorrectSeperator + itemPathWithCorrectSeparator;

            return new GitItem(
                GitSolution,
                _repositoryFactory,
                null,
                absoluteFilePath
            );
        }

        private GitItem CreateDiffedObject(
            IRepository gitRepo,
            TreeEntryChanges treeEntryChange)
        {
            var itemPathWithCorrectSeparator = treeEntryChange.Path.Replace('/', Path.DirectorySeparatorChar);
            var repoPathWithCorrectSeperator = gitRepo.Info.WorkingDirectory.Replace('/', Path.DirectorySeparatorChar);

            var absoluteFilePath = repoPathWithCorrectSeperator + itemPathWithCorrectSeparator;
            var oldAbsoluteFilePath = treeEntryChange.Status == ChangeKind.Renamed ? treeEntryChange.OldPath.Replace('/', Path.DirectorySeparatorChar) : string.Empty;

            return new GitItem(
                GitSolution,
                _repositoryFactory,
                ComparisonConfig.ReferenceObject,
                absoluteFilePath,
                oldAbsoluteFilePath
            );
        }

        private static void AssertValidReference(IRepository repository, GitReference<GitCommitObject> reference)
        {
            if (reference == null)
            {
                throw new NothingToCompareException();
            }

            var branchesInRepo = repository.Branches.Select(branch => branch.FriendlyName);
            var activeBranch = repository.Head?.FriendlyName;

            if (repository.GetTargetCommit(reference) == null)
            {
                throw new RepositoryTargetNotFoundException();
            }
            if (string.IsNullOrEmpty(activeBranch) || !branchesInRepo.Contains(activeBranch))
            {
                throw new HeadNotFoundException();
            }
        }

        private static IEnumerable<TreeEntryChanges> GetViewableChanges(TreeChanges branchDiffResult)
        {
            return branchDiffResult.Modified
                .Concat(branchDiffResult.Added)
                .Concat(branchDiffResult.Renamed);
        }

        private static IEnumerable<TreeEntryChanges> GetFileDiffViewableChanges(TreeChanges branchDiffResult)
        {
            return branchDiffResult.Modified
                .Concat(branchDiffResult.Added)
                .Concat(branchDiffResult.Renamed)
                .Concat(branchDiffResult.Conflicted)
                .Concat(branchDiffResult.Copied);
        }


        private static IEnumerable<string> GetExtraFilesToAdd(RepositoryStatus repositoryStatus)
        {
            return repositoryStatus.Modified
                .Concat(repositoryStatus.Untracked)
                .Concat(repositoryStatus.Staged)
                .Concat(repositoryStatus.RenamedInWorkDir)
                .Concat(repositoryStatus.Added).Select(x => x.FilePath);
        }

        private HashSet<GitItem> CollectTreeChanges(IRepository repository)
        {
            Commit headCommit = repository.Head.Tip;
            Commit referenceCommit = SelectReferenceCommit(repository, headCommit);

            TreeChanges branchDiffResult = CreateTreeChanges(repository, headCommit, referenceCommit);

            var changes = GetViewableChanges(branchDiffResult);
            var changeset = changes.Select(x => CreateDiffedObject(repository, x)).ToHashSet();
            return changeset;
        }

        private Commit SelectReferenceCommit(IRepository repository, Commit headCommit)
        {
            var targetCommit = RepositoryExtensions.GetTargetCommit(repository, ComparisonConfig.ReferenceObject);
            
            // This is necessary if an only if there are modified files which already exist in your local worktree.
            // The new files added in main since we branched off, are going to be missing in local sources and therefore ignored anyway.
            // Likewise, we don't display deletions.
            Commit mergeBase = repository.ObjectDatabase.FindMergeBase(headCommit, targetCommit);
            if (mergeBase != null)
            {
                targetCommit = mergeBase;
            }

            return targetCommit;
        }

        private TreeChanges CreateTreeChanges(IRepository repository, Commit headCommit, Commit targetCommit)
        {
            var compareOptions = new CompareOptions
            {
                Algorithm = DiffAlgorithm.Minimal,
                IncludeUnmodified = false,
            };

            return repository.Diff.Compare<TreeChanges>(
                targetCommit.Tree,
                headCommit.Tree,
                compareOptions);
        }

        private static IEnumerable<string> GetExtraFilesToRemove(RepositoryStatus repositoryStatus)
        {
            return repositoryStatus.Removed.Select(x => x.FilePath);
        }
    }
}

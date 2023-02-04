using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using LibGit2Sharp;

namespace GitTreeFilter.Core
{
    public interface ISolutionRepository
    {
        GitSolution GitSolution { get; }
        IComparisonConfig ComparisonConfig { get; }
        IEnumerable<GitBranch> Branches { get; }
        GitChangeset Changeset { get; }
        bool TryReadItem(string path, out GitItem item);
        GitReference<GitCommitObject> GitReference { get; set; }
        IEnumerable<GitCommit> GetRecentCommits(int number = 50);
        IEnumerable<GitTag> GetRecentTags(int number = 50);

        /// <summary>
        /// Attempts to load a reference from the repository, restoring its full details.
        /// If it is not possible to load the original refrence, a git commit will be returned.
        /// Returns false only in the case the commit no longer exists in the repository.
        /// </summary>
        /// <param name="gitReference">Dehydrated reference to be hydrated</param>
        /// <returns>Hydrated reference of original type or a hydrated commit, if original object no longer resolvable</returns>
        bool TryHydrate(GitReference<GitCommitObject> gitReference, out GitReference<GitCommitObject> hydratedReference);
        bool TryGetGitBranchByName(string branchName, out GitBranch branch);
    }

    internal class SolutionRepository : ISolutionRepository
    {
        public GitSolution GitSolution { get; }

        public IComparisonConfig ComparisonConfig { get; }

        public GitReference<GitCommitObject> GitReference { get; set; }

        private readonly GitRepositoryFactory _repositoryFactory;

        public IEnumerable<GitBranch> Branches
        {
            get
            {
                using (var repository = _repositoryFactory.Create(GitSolution))
                {
                    return repository.Branches
                        .Where(p => !ComparisonConfig.OriginRefsOnly || p.IsRemote)
                        .Select(x => new GitBranch(x.Tip.ToGitCommitObject(), x.FriendlyName))
                        .ToList();
                }
            }
        }

        public GitChangeset Changeset
        {
            get
            {
                using (var repository = _repositoryFactory.Create(GitSolution))
                {
                    AssertValidRepository(repository, GitReference);
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
        }

        private HashSet<GitItem> CollectTreeChanges(IRepository repository)
        {
            var compareOptions = new CompareOptions
            {
                Algorithm = DiffAlgorithm.Minimal,
                IncludeUnmodified = false,
            };

            var targetCommit = RepositoryExtensions.GetTargetCommit(repository, GitReference);

            var branchDiffResult = repository.Diff.Compare<TreeChanges>(
               targetCommit.Tree,
                repository.Head.Tip.Tree,
                compareOptions);

            var changes = GetViewableChanges(branchDiffResult);
            var changeset = changes.Select(x => CreateDiffedObject(repository, x)).ToHashSet();
            return changeset;
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

        public IEnumerable<GitCommit> GetRecentCommits(int number = 50)
        {
            using (var repository = _repositoryFactory.Create(GitSolution))
            {
                // TODO try to do this, as it shows all commits on the current branch
                //git rev-list --first-parent master
                //var firstParent = repository.Refs[""].

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
        }

        public IEnumerable<GitTag> GetRecentTags(int number = 50)
        {
            using (var repository = _repositoryFactory.Create(GitSolution))
            {
                return repository.Tags
                     .Where(x => !x.PeeledTarget.IsMissing)
                     .Take(number)
                     .Select(x =>
                     {
                         try
                         {
                             var commit = repository.Lookup<Commit>(x.Reference.TargetIdentifier);
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
        }

        public bool TryGetGitBranchByName(string branchName, out GitBranch branch)
        {
            using (var repository = _repositoryFactory.Create(GitSolution))
            {
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
        }

        public bool TryReadItem(string path, out GitItem item)
        {
            item = null;
            using (var repository = _repositoryFactory.Create(GitSolution))
            {
                var compareOptions = new CompareOptions
                {
                    Algorithm = DiffAlgorithm.Minimal,
                    IncludeUnmodified = false,
                };

                var targetCommit = RepositoryExtensions.GetTargetCommit(repository, GitReference);

                var branchDiffResult = repository.Diff.Compare<TreeChanges>(
                   targetCommit.Tree,
                    DiffTargets.WorkingDirectory, Enumerable.Repeat(path, 1));

                var changes = GetFileDiffViewableChanges(branchDiffResult);
                item = changes.Select(x => CreateDiffedObject(repository, x)).FirstOrDefault();

                if(item == default)
                {
                    return false;
                }
            }
            return true;
        }

        public bool TryHydrate(GitReference<GitCommitObject> gitReference, out GitReference<GitCommitObject> hydratedReference)
        {
            using (var repository = _repositoryFactory.Create(GitSolution))
            {
                return RepositoryExtensions.TryHydrate(repository, gitReference, out hydratedReference, ComparisonConfig);
            }
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
                    GitReference,
                    absoluteFilePath,
                    oldAbsoluteFilePath
            );
        }

        private static void AssertValidRepository(IRepository repository, GitReference<GitCommitObject> reference)
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
            if (activeBranch.Equals(reference))
            {
                throw new NothingToCompareException();
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

        private static IEnumerable<string> GetExtraFilesToRemove(RepositoryStatus repositoryStatus)
        {
            return repositoryStatus.Removed.Select(x => x.FilePath);
        }
    }
}

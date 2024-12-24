using System.Collections.Generic;
using System.Linq;
using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Repositories;

public interface ITestRepository
{
    GitSolution Solution { get; }

    /// <summary>
    /// Currently checked out commit.
    /// </summary>
    GitCommit Head { get;  }

    /// <summary>
    /// All unique commit objects in the repository which belong to all known branches.
    /// </summary>
    IReadOnlyList<GitCommitObject> CommitObjects { get; }

    /// <summary>
    /// All unique commits in the repository which belong to all known branches.
    /// </summary>
    IReadOnlyList<GitCommit> AllCommits { get; }

    /// <summary>
    /// Commits which are in the history of the currently checked out branch.
    /// </summary>
    IReadOnlyList<GitCommit> HeadCommits { get; }

    /// <summary>
    /// All known branches.
    /// </summary>
    IReadOnlyList<GitBranch> Branches { get; }

    /// <summary>
    /// All known tags.
    /// </summary>
    IReadOnlyList<GitTag> Tags { get; }

    string Name { get ; }
}

public static class ITestRepositoryExt
{
    public static GitCommit CommitByMessage(this ITestRepository repository, string name) =>
        repository.AllCommits.First(x => string.Equals(name, x.ShortMessage, System.StringComparison.Ordinal));

    public static GitCommit TipOfBranch(this ITestRepository repository, string name) => 
        repository.Branches.First(x => string.Equals(name, x.FriendlyName, System.StringComparison.Ordinal)).Tip();
}

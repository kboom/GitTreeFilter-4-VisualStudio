using System.Collections.Generic;
using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Repositories
{
    public interface ITestRepository
    {
        GitSolution Solution { get; }

        GitCommit Head { get;  }

        IReadOnlyList<GitCommitObject> CommitObjects { get; }

        IReadOnlyList<GitCommit> Commits { get; }

        IReadOnlyList<GitBranch> Branches { get; }

        string Name { get ; }
    }
}

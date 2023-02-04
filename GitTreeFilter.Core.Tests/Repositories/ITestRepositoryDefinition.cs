using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Repositories
{
    public interface ITestRepository
    {
        GitSolution Solution { get; }

        IReadOnlyList<GitCommitObject> CommitObjects { get; }

        IReadOnlyList<GitCommit> Commits { get; }

        IReadOnlyList<GitBranch> Branches { get; }

        string Name { get ; }
    }
}

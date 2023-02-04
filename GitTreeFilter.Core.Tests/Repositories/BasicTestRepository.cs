using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Repositories
{
    public sealed class BasicTestRepository : ITestRepository
    {
        public GitSolution Solution => new(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../TestResources/TestRepository")));

        public IReadOnlyList<GitCommitObject> CommitObjects => Array.AsReadOnly(new GitCommitObject[] {
            new ("38bd60abc0546bf92c54802dfa810480c9f4a96f", "Initialized dotnet"),
            new ("bd6eb8b7f0879bfbf6f33863a655b2d0f8084ef5", "Initial commit.")
        });

        public IReadOnlyList<GitCommit> Commits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

        public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
            new GitBranch(CommitObjects.ElementAt(0), "main"),
            new GitBranch(CommitObjects.ElementAt(0), "origin/HEAD"),
            new GitBranch(CommitObjects.ElementAt(0), "origin/main")
        });

        public string Name => nameof(BasicTestRepository);
    }
}
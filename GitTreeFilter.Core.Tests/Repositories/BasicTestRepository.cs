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
            new ("1b8b440fa493c359a46224051c880a64c17af6b9", "Added Class3.cs"),
            new ("4267eeadbc291fee1bcd078a5f719113c96203be", "Removes Class2.cs"),
            new ("f256a5903c765bab6b4cf8ca3c869210690cc986", "Changed Class2.cs"),
            new ("c51eb9ff538a4af7b653d3fa35ccb9d6fc9321b4", "Added Class2"),
            new ("38bd60abc0546bf92c54802dfa810480c9f4a96f", "Initialized dotnet"),
            new ("bd6eb8b7f0879bfbf6f33863a655b2d0f8084ef5", "Initial commit.")
        });

        public IReadOnlyList<GitCommit> Commits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

        public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
            new GitBranch(CommitObjects.ElementAt(0), "main"),
            new GitBranch(CommitObjects.ElementAt(0), "origin/HEAD"),
            new GitBranch(CommitObjects.ElementAt(0), "origin/main")
        });

        public IReadOnlyList<GitTag> Tags => Array.AsReadOnly(new GitTag[] {
            new GitTag(CommitObjects.ElementAt(2), "tag1"),
            new GitTag(CommitObjects.ElementAt(4), "tag2")
        });


        public string Name => nameof(BasicTestRepository);

        public GitCommit Head => Commits[0];
    }
}
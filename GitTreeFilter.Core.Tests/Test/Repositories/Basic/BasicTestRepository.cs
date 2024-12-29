using GitTreeFilter.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Basic;

/// <summary>
/// The simple repository to validate basic functionality.
/// 
/// This repository contains a "feature" branch created from the "main" branch. 
/// The "feature" branch has one additional commit not yet merged into the "main" branch.
/// Similarly, the "main" branch has one additional commit not present in the "feature" branch.
/// The currently checked out branch is the "feature" branch, and we are comparing it to various branches, commits, and tags in this repository.
/// </summary>
public sealed class BasicTestRepository : ITestRepository
{
    public GitSolution Solution => new(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../TestResources/TestRepository")));

    public IReadOnlyList<GitCommitObject> CommitObjects => Array.AsReadOnly(new GitCommitObject[] {
        new (COMMIT_014F5B, "Added FeatureClass.cs"),
        new (COMMIT_E0EE46, "Added Class4.cs"),
        new (COMMIT_1B8B44, "Added Class3.cs"),
        new (COMMIT_4267EE, "Removes Class2.cs"),
        new (COMMIT_F256A5, "Changed Class2.cs"),
        new (COMMIT_C51EB9, "Added Class2"),
        new (COMMIT_38BD60, "Initialized dotnet"),
        new (COMMIT_BD6EB8, "Initial commit.")
    });

    public IReadOnlyList<GitCommit> AllCommits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

    public IReadOnlyList<GitCommit> HeadCommits => Array.AsReadOnly(CommitObjects.Except(new List<GitCommitObject>() { CommitObjects[1] }).Select(x => new GitCommit(x)).ToArray());

    public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_014F5B).Reference, "feature"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_E0EE46).Reference, "main"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_014F5B).Reference, "origin/feature"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_E0EE46).Reference, "origin/HEAD"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_E0EE46).Reference, "origin/main"),
    });

    public IReadOnlyList<GitTag> Tags => Array.AsReadOnly(new GitTag[] {
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_E0EE46).Reference, "tag3"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_F256A5).Reference, "tag1"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_38BD60).Reference, "tag2"),
    });

    public string Name => nameof(BasicTestRepository);

    public GitCommit Head => AllCommits[0];

    private const string COMMIT_014F5B = "014f5b75defe9e24d265243071c85d159d6a4eeb";
    private const string COMMIT_E0EE46 = "e0ee464a90d8639831b86a2e4fbbeb270097e483";
    private const string COMMIT_1B8B44 = "1b8b440fa493c359a46224051c880a64c17af6b9";
    private const string COMMIT_4267EE = "4267eeadbc291fee1bcd078a5f719113c96203be";
    private const string COMMIT_F256A5 = "f256a5903c765bab6b4cf8ca3c869210690cc986";
    private const string COMMIT_C51EB9 = "c51eb9ff538a4af7b653d3fa35ccb9d6fc9321b4";
    private const string COMMIT_38BD60 = "38bd60abc0546bf92c54802dfa810480c9f4a96f";
    private const string COMMIT_BD6EB8 = "bd6eb8b7f0879bfbf6f33863a655b2d0f8084ef5";
}

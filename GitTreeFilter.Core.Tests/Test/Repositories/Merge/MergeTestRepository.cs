using GitTreeFilter.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Merge;

/// <summary>
/// The repository to validate behavior for all types of merging.
/// 
/// This repository contains a "feature" branch created from the "main" branch at some point in its history.
/// The "feature" branch has a few additional commits not yet merged into the "main" branch.
/// In addition, the "feature" branch has merged new changes from "main" in a merge commit.
/// The currently checked out branch is the "feature" branch, and we are comparing it to various branches, commits, and tags in this repository.
/// </summary>
public sealed class MergeTestRepository : ITestRepository
{
    public GitSolution Solution => new(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../TestResources/TestRepository2")));

    public IReadOnlyList<GitCommitObject> CommitObjects => Array.AsReadOnly(new GitCommitObject[] {
        new (COMMIT_BBED6F, "Changed: [MainClass2.cs, MainFolderClass2.cs] Added: [MainClass3.cs, MainClass4.cs, MainFolderClass5.cs]"),
        new (COMMIT_BB6FDC, "Changed: [MainClass1.cs] Added: [FeatureClass2.cs]"),
        new (COMMIT_6214D8, "Merge branch 'main' into feature"),
        new (COMMIT_BCA8A8, "Changed: [MainClass3.cs] Added: [MainFolderClass4.cs] Deleted: [MainClass3.cs]"),
        new (COMMIT_FEAB37, "Deleted: [MainFolderClass3.cs]"),
        new (COMMIT_2DF026, "Changed: [MainFolderClass1.cs] Added: [FeatureClass1.cs]"),
        new (COMMIT_CBA89D, "Changed: [MainClass2.cs] Added: [MainClass3.cs, MainFolderClass3.cs]"),
        new (COMMIT_A9BED4, "Root commit")
    });

    public IReadOnlyList<GitCommit> AllCommits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

    public IReadOnlyList<GitCommit> HeadCommits => ITestRepositoryExt.CommitsBySha(this, new string[] {
        COMMIT_BB6FDC,
        COMMIT_6214D8,
        COMMIT_FEAB37,
        COMMIT_2DF026,
        COMMIT_CBA89D,
        COMMIT_A9BED4
    });

    public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_BBED6F).Reference, "main"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_BB6FDC).Reference, "feature"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_BBED6F).Reference, "origin/HEAD"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_BBED6F).Reference, "origin/main"),
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_BB6FDC).Reference, "origin/feature")
    });

    public IReadOnlyList<GitTag> Tags => Array.AsReadOnly(new GitTag[] {
        new GitTag(new GitCommitObject(COMMIT_58C98C, "Add classes on main"), "fork"), // This tag points to a commit which no longer belongs to any known branch
        new(ITestRepositoryExt.CommitBySha(this, COMMIT_6214D8).Reference, "merge")
    });

    public string Name => nameof(MergeTestRepository);

    public GitCommit Head => ITestRepositoryExt.TipOfBranch(this, "origin/feature");

    private const string COMMIT_BB6FDC = "bb6fdc837541bf49cb4c24cada15611d1bdab6b5";
    private const string COMMIT_6214D8 = "6214d8928e252a8d90782c061853ebbe35ee6bd2";
    private const string COMMIT_FEAB37 = "feab371b66122adfc545c45b7b1e88c2eaa6e365";
    private const string COMMIT_2DF026 = "2df026a86fb4d859595e06d7606ca99253a4a5c7";
    private const string COMMIT_CBA89D = "cba89dac9e4cbad12ab3595f15b73f1cde01d308";
    private const string COMMIT_A9BED4 = "a9bed46bb7aa27123326043531ef7228a5b1a257";
    private const string COMMIT_BBED6F = "bbed6fbcdaa4aa04e12e3b22740dbc8753acaaf4";
    private const string COMMIT_BCA8A8 = "bca8a805ccf19ae02e69adcda6293f9ad1ec1182";
    private const string COMMIT_58C98C = "58c98c2aeeab1b4c554de989d9c2c1262eb608d5"; // This commit purposefully does not belong to any branch
}
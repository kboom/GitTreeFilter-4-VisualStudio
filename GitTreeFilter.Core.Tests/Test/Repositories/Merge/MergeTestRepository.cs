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
        new ("bbed6fbcdaa4aa04e12e3b22740dbc8753acaaf4", "Changed: [MainClass2.cs, MainFolderClass2.cs]\r\n    Added: [MainClass3.cs, MainClass4.cs, MainFolderClass5.cs]"),
        new ("bb6fdc837541bf49cb4c24cada15611d1bdab6b5", "Changed: [MainClass1.cs]\r\n    Added: [FeatureClass2.cs]"),
        new ("6214d8928e252a8d90782c061853ebbe35ee6bd2", "Merge branch 'main' into feature"),
        new ("bca8a805ccf19ae02e69adcda6293f9ad1ec1182", "Changed: [MainClass3.cs]\r\n    Added: [MainFolderClass4.cs]\r\n    Deleted: [MainClass3.cs]"),
        new ("feab371b66122adfc545c45b7b1e88c2eaa6e365", "Deleted: [MainFolderClass3.cs]"),
        new ("2df026a86fb4d859595e06d7606ca99253a4a5c7", "Changed: [MainFolderClass1.cs]\r\n    Added: [FeatureClass1.cs]"),
        new ("cba89dac9e4cbad12ab3595f15b73f1cde01d308", "Changed: [MainClass2.cs]\r\n    Added: [MainClass3.cs, MainFolderClass3.cs]"),
        new ("a9bed46bb7aa27123326043531ef7228a5b1a257", "Root commit")
    });

    public IReadOnlyList<GitCommit> AllCommits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

    public IReadOnlyList<GitCommit> HeadCommits => AllCommits.Skip(1).ToList();

    public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
        new(CommitObjects.ElementAt(1), "feature"),
        new(CommitObjects.ElementAt(1), "origin/feature"),
        new(CommitObjects.ElementAt(1), "origin/HEAD"),
        new(CommitObjects.ElementAt(0), "main"),
        new(CommitObjects.ElementAt(0), "origin/main"),
    });

    public IReadOnlyList<GitTag> Tags => Array.AsReadOnly(new GitTag[] {
        new(ITestRepositoryExt.CommitBySha(this, "6214d8928e252a8d90782c061853ebbe35ee6bd2").Reference, "merge"),
        new(ITestRepositoryExt.CommitBySha(this, "cba89dac9e4cbad12ab3595f15b73f1cde01d308").Reference, "fork"),
    });


    public string Name => nameof(MergeTestRepository);

    public GitCommit Head => AllCommits[0];
}
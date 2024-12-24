using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test.Repositories.Basic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Merge
{
    /// <summary>
    /// The repository to validate behavior for all types of merging.
    /// 
    /// This repository contains a "feature1" branch created from the "main" branch. 
    /// The "feature1" branch has one additional commit not yet merged into the "main" branch.
    /// In addition, the "feature1" branch has merged new changes from "main" in a merge commit.
    /// The currently checked out branch is the "feature" branch1, and we are comparing it to various branches, commits, and tags in this repository.
    /// </summary>
    public sealed class MergeTestRepository : ITestRepository
    {
        public GitSolution Solution => new(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../TestResources/TestRepository2")));

        public IReadOnlyList<GitCommitObject> CommitObjects => Array.AsReadOnly(new GitCommitObject[] {
            new ("aa72505691ac8ea4bb3ef57c2bff6fa980a61a8d", "Class4 on main"),
            new ("cd4a16179a6e5e22dda963f49b179a4125e1ef1d", "New commit on feature1 after merge of main"),
            new ("c2d40f96632bf782777c682a4aa525dd5e7db4df", "Merge branch 'main' into feature1"),
            new ("3c94c054a801b26c6494b42cc86bb07ac7914e00", "Add solution on main"),
            new ("7a3e3394e41f41bf1c9d64b65180a6f23bd83366", "Modifications on main"),
            new ("c065d0a681c624a0210e16f05522b19b85a5f70e", "Add classes on feature 1"),
            new ("58c98c2aeeab1b4c554de989d9c2c1262eb608d5", "Add classes on main"),
            new ("625b7ff0063e49b4984873d30148704992313915", "Master commit 1")
        });

        public IReadOnlyList<GitCommit> AllCommits => Array.AsReadOnly(CommitObjects.Select(x => new GitCommit(x)).ToArray());

        public IReadOnlyList<GitCommit> HeadCommits => AllCommits;

        public IReadOnlyList<GitBranch> Branches => Array.AsReadOnly(new GitBranch[] {
            new(CommitObjects.ElementAt(1), "feature1"),
            new(CommitObjects.ElementAt(0), "main"),
            new(CommitObjects.ElementAt(1), "origin/feature1"),
            new(CommitObjects.ElementAt(0), "origin/HEAD"),
            new(CommitObjects.ElementAt(0), "origin/main"),
        });

        public IReadOnlyList<GitTag> Tags => Array.AsReadOnly(new GitTag[] {
            new(CommitObjects.ElementAt(3), "tag1"),
            new(CommitObjects.ElementAt(2), "tag2")
        });


        public string Name => nameof(BasicTestRepository);

        public GitCommit Head => AllCommits[0];
    }
}
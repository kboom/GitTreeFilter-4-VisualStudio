using System;
using System.Collections.Generic;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Merge;

internal class MergeRepositoryChangesetDataSourceAttribute : AbstractChangesetDataSource
{
    public override ITestRepository Repository => TestRepositories.Merge;

    public override IReadOnlyList<ChangesetDescriptor> GetDescriptors() => new List<ChangesetDescriptor>
    {
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "All changes in HEAD since fork point by commit",
                ReferenceObject = Repository.CommitBySha("cba89dac9e4cbad12ab3595f15b73f1cde01d308"),
            },
            FilesInChangeset = new string[]
            {
                "MainClass1.cs",
                "MainClass2.cs",
                "MainFolder\\MainFolderClass1.cs",
                "Feature\\FeatureClass1.cs",
                "Feature\\FeatureClass2.cs",
                "MainFolder\\MainFolderClass4.cs"
            }
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "All changes in HEAD compared to main",
                ReferenceObject = Repository.TipOfBranch("origin/main"),
            },
            FilesInChangeset = new string[]
            {
                "MainClass1.cs",
                "MainFolder\\MainFolderClass1.cs",
                "Feature\\FeatureClass1.cs",
                "Feature\\FeatureClass2.cs",
            }
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "Changes since last merge by commit",
                ReferenceObject = Repository.CommitByMessage("Merge branch 'main' into feature"),
            },
            FilesInChangeset = new string[]
            {
                "MainClass1.cs",
                "Feature\\FeatureClass2.cs"
            }
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "Changes since last merge by tag",
                ReferenceObject = Repository.TagByName("merge"),
            },
            FilesInChangeset = new string[]
            {
                "MainClass1.cs",
                "Feature\\FeatureClass2.cs"
            }
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "Changes to tagged commit no longer in history",
                ReferenceObject = Repository.TagByName("fork"),
            },
            // This correctly brings up the entire worktree
            FilesInChangeset = new string[]
            {
                "README.md",
                "GitTreeFilter-testrepo-2.sln",
                "MainClass1.cs",
                "MainClass2.cs",
                "MainFolder\\MainFolderClass1.cs",
                "MainFolder\\MainFolderClass2.cs",
                "MainFolder\\MainFolderClass4.cs",
                "Feature\\FeatureClass1.cs",
                "Feature\\FeatureClass2.cs"
            }
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "No changes to feature",
                ReferenceObject = Repository.TipOfBranch("feature"),
            },
            FilesInChangeset = Array.Empty<string>()
        },
        new()
        {
            ComparisonConfig = new TestComparisonConfig()
            {
                TestName = "No changes to origin/feature",
                ReferenceObject = Repository.TipOfBranch("origin/feature"),
            },
            FilesInChangeset = Array.Empty<string>()
        }
    };
}

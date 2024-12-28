using System.Collections.Generic;
using System.Linq;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Basic
{
    internal class BasicRepositoryChangesetDataSourceAttribute : AbstractChangesetDataSource
    {
        public override ITestRepository Repository => TestRepositories.Basic;

        public override IReadOnlyList<ChangesetDescriptor> GetDescriptors() => new List<ChangesetDescriptor>
        {
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "All changes in HEAD",
                    ReferenceObject = Repository.HeadCommits.Last()
                },
                FilesInChangeset = new string[]
                {
                    "Class1.cs",
                    "Class3.cs",
                    "FeatureClass.cs",
                    "GitTreeFilter-testrepo.csproj"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Commit before current in HEAD",
                    ReferenceObject = Repository.CommitByMessage("Added Class3.cs")
                },
                FilesInChangeset = new string[]
                {
                    "FeatureClass.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Flattening added and removed classes",
                    ReferenceObject = Repository.CommitByMessage("Added Class2")
                },
                FilesInChangeset = new string[]
                {
                    "Class3.cs",
                    "FeatureClass.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Commit missing in HEAD with PinToMergeHead OFF",
                    ReferenceObject = Repository.CommitByMessage("Added Class4.cs"),
                    PinToMergeHead = false
                },
                FilesInChangeset = new string[]
                {
                    "FeatureClass.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Commit missing in HEAD with PinToMergeHead ON",
                    ReferenceObject = Repository.CommitByMessage("Added Class4.cs"),
                    PinToMergeHead = true
                },
                FilesInChangeset = new string[]
                {
                    "FeatureClass.cs"
                }
            }
        };
    }
}

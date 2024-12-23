using System.Collections.Generic;
using System.Linq;
using GitTreeFilter.Core.Tests.Repositories;
using GitTreeFilter.Core.Tests.Test;

namespace GitTreeFilter.Core.Tests.DataSource
{
    internal class FirstRepositoryChangesetDataSourceAttribute : AbstractChangesetDataSource
    {
        public override IReadOnlyList<ChangesetDescriptor> GetDescriptors() => new List<ChangesetDescriptor>
        {
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    ReferenceObject = TestRepositories.First.HeadCommits.Last()
                },
                Repository = TestRepositories.First,
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
                    ReferenceObject = TestRepositories.First.CommitByMessage("Added Class3.cs")
                },
                Repository = TestRepositories.First,
                FilesInChangeset = new string[]
                {
                    "FeatureClass.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    ReferenceObject = TestRepositories.First.CommitByMessage("Added Class2")
                },
                Repository = TestRepositories.First,
                FilesInChangeset = new string[]
                {
                    "Class3.cs",
                    "FeatureClass.cs"
                }
            }
        };
    }
}

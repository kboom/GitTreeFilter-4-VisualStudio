using System.Collections.Generic;
using System.Linq;
using GitTreeFilter.Core.Tests.Repositories;

namespace GitTreeFilter.Core.Tests.DataSource
{
    internal class FirstRepositoryChangesetDataSourceAttribute : AbstractChangesetDataSource
    {
        public override IReadOnlyList<ChangesetDescriptor> GetDescriptors() => new List<ChangesetDescriptor>
        {
            new()
            {
                Repository = TestRepositories.First,
                Reference = TestRepositories.First.HeadCommits.Last(),
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
                Repository = TestRepositories.First,
                Reference = TestRepositories.First.CommitByMessage("Added Class3.cs"),
                FilesInChangeset = new string[]
                {
                    "FeatureClass.cs"
                }
            },
            new()
            {
                Repository = TestRepositories.First,
                Reference = TestRepositories.First.CommitByMessage("Added Class2"),
                FilesInChangeset = new string[]
                {
                    "Class3.cs",
                    "FeatureClass.cs"
                }
            }
        };
    }
}

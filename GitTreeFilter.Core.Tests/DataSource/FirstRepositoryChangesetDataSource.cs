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
                Reference = TestRepositories.First.Commits.Last(),
                FilesInChangeset = new string[]
                {
                    "Class1.cs",
                    "Class3.cs",
                    "GitTreeFilter-testrepo.csproj"
                }
            },
            new()
            {
                Repository = TestRepositories.First,
                Reference = TestRepositories.First.Commits[TestRepositories.First.Commits.Count - 2],
                FilesInChangeset = new string[]
                {
                    "Class3.cs"
                }
            },
            new()
            {
                Repository = TestRepositories.First,
                Reference = TestRepositories.First.Commits[TestRepositories.First.Commits.Count - 3],
                FilesInChangeset = new string[]
                {
                    "Class3.cs"
                }
            }
        };
    }
}

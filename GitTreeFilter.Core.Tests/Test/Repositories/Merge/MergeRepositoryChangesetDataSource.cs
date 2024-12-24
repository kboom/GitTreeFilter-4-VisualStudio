using System.Collections.Generic;
using System.Linq;

namespace GitTreeFilter.Core.Tests.Test.Repositories.Merge
{
    internal class MergeRepositoryChangesetDataSourceAttribute : AbstractChangesetDataSource
    {
        private readonly static ITestRepository Repository = TestRepositories.Merge;

        public override IReadOnlyList<ChangesetDescriptor> GetDescriptors() => new List<ChangesetDescriptor>
        {
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "All changes in HEAD",
                    ReferenceObject = Repository.HeadCommits.Last()
                },
                Repository = Repository,
                FilesInChangeset = new string[]
                {
                    "ClassesOnFeature1\\Feature1Class1.cs",
                    "ClassesOnFeature1\\Feature1Class2.cs",
                    "ClassesOnFeature1\\Feature1Class3.cs",
                    "ClassesOnMain\\Class1.cs",
                    "ClassesOnMain\\Class2.cs",
                    "ClassesOnMain\\Class3.cs",
                    "GitTreeFilter-testrepo-2.sln"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Changes vs main",
                    ReferenceObject = Repository.TipOfBranch("main")
                },
                Repository = Repository,
                FilesInChangeset = new string[]
                {
                    "ClassesOnFeature1\\Feature1Class1.cs",
                    "ClassesOnFeature1\\Feature1Class2.cs",
                    "ClassesOnFeature1\\Feature1Class3.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Changes vs fork point",
                    ReferenceObject = Repository.CommitByMessage("Add classes on main")
                },
                Repository = Repository,
                FilesInChangeset = new string[]
                {
                    "ClassesOnFeature1\\Feature1Class1.cs",
                    "ClassesOnFeature1\\Feature1Class2.cs",
                    "ClassesOnFeature1\\Feature1Class3.cs",
                    "ClassesOnMain\\Class1.cs", // Thanks to modification in 'Modifications on main'
                    "ClassesOnMain\\Class3.cs", // Thanks to being added in 'Modifications on main'
                    "GitTreeFilter-testrepo-2.sln"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Compare to a missing commit",
                    ReferenceObject = Repository.CommitByMessage("Class4 on main")
                },
                Repository = Repository,
                FilesInChangeset = new string[]
                {
                    "ClassesOnFeature1\\Feature1Class1.cs",
                    "ClassesOnFeature1\\Feature1Class2.cs",
                    "ClassesOnFeature1\\Feature1Class3.cs"
                }
            },
            new()
            {
                ComparisonConfig = new TestComparisonConfig()
                {
                    TestName = "Compare to newer branch",
                    ReferenceObject = Repository.TipOfBranch("main")
                },
                Repository = Repository,
                FilesInChangeset = new string[]
                {
                    "ClassesOnFeature1\\Feature1Class1.cs",
                    "ClassesOnFeature1\\Feature1Class2.cs",
                    "ClassesOnFeature1\\Feature1Class3.cs"
                }
            }
        };
    }
}

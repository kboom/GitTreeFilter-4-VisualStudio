using System.Collections.Generic;

namespace GitTreeFilter.Core.Tests.Test.Repositories
{
    internal class AllRepositoriesDataAttribute : AbstractRepositoryDataSource
    {
        public override IReadOnlyList<ITestRepository> GetTestRepositories()
        {
            return new List<ITestRepository>() {
                TestRepositories.Basic,
                TestRepositories.Merge
            }.AsReadOnly();
        }
    }
}

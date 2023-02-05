using System.Collections.Generic;
using GitTreeFilter.Core.Tests.Repositories;

namespace GitTreeFilter.Core.Tests.DataSource
{
    internal class AllRepositoriesDataAttribute : AbstractRepositoryDataSource
    {
        public override IReadOnlyList<ITestRepository> GetTestRepositories() => new List<ITestRepository>() { TestRepositories.First }.AsReadOnly();
    }
}

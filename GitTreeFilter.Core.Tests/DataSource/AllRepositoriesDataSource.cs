using GitTreeFilter.Core.Tests.Repositories;
using System.Collections.Generic;

namespace GitTreeFilter.Core.Tests.DataSource
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

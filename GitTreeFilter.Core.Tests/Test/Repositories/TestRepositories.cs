using GitTreeFilter.Core.Tests.Test.Repositories.Basic;
using GitTreeFilter.Core.Tests.Test.Repositories.Merge;

namespace GitTreeFilter.Core.Tests.Test.Repositories
{
    public static class TestRepositories
    {
        public static readonly ITestRepository Basic = new BasicTestRepository();
        public static readonly ITestRepository Merge = new MergeTestRepository();
    }
}

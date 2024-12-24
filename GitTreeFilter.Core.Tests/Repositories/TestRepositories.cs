namespace GitTreeFilter.Core.Tests.Repositories
{
    public static class TestRepositories
    {
        public static readonly ITestRepository Basic = new BasicTestRepository();
        public static readonly ITestRepository Merge = new MergeTestRepository();
    }
}

using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    public abstract class SolutionRepositoryTest
    {
        public TestComparisonConfig ComparisonConfig = new();

        protected ISolutionRepository CreateSolutionRepository(ITestRepository testRepository)
        {
            return SolutionRepositoryFactory.CreateSolutionRepository(testRepository.Solution, ComparisonConfig);
        }
    }

    public class TestComparisonConfig : IComparisonConfig
    {
        public bool OriginRefsOnly => false;
    }
}
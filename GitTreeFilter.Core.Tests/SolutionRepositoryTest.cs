using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    public abstract class SolutionRepositoryTest : IComparisonConfig
    {
        protected ISolutionRepository CreateSolutionRepository(ITestRepository testRepository)
        {
            return SolutionRepositoryFactory.CreateSolutionRepository(testRepository.Solution, this);
        }

        public GitReference<GitCommitObject> ReferenceObject { get; set; }

        public bool OriginRefsOnly { get; set; }

        public bool PinToMergeHead { get; set; }
    }
}
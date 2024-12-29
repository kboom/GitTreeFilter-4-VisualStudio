using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Tests.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.Branches")]
    public class BranchesTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void IsCorrect(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var branches = solutionRepository.Branches;

            // then
            branches.Should()
                .NotContainNulls()
                .And
                .HaveCountGreaterThan(0)
                .And
                .AllSatisfy(x =>
                {
                    x.Reference.Should().NotBeNull();
                    x.FriendlyName.Should().NotBeNullOrEmpty();
                    x.Reference.Should().NotBeNull();
                    x.Reference.Sha.Should().NotBeNullOrEmpty();
                    x.Reference.ShortMessage.Should().NotBeNullOrEmpty();
                });
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ResolvesToCorrectBranches(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var branches = solutionRepository.Branches;

            // then
            using (new AssertionScope())
            {
                branches.Should().HaveCount(testRepository.Branches.Count);

                branches.Should().BeEquivalentTo(testRepository.Branches, options => options
                    .Including(branch => branch.FriendlyName)
                    .Including(branch => branch.Reference.Sha)
                    .Including(branch => branch.Reference.ShortMessage)
                    .RespectingRuntimeTypes());
            }
        }
    }
}

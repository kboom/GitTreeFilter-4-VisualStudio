using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
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
                branches.Select(b => b.FriendlyName)
                    .Should()
                    .Equal(testRepository.Branches.Select(x => x.FriendlyName));

                branches.Select(b => b.Reference.Sha)
                    .Should()
                    .Equal(testRepository.Branches.Select(x => x.Reference.Sha));

                branches.Select(b => b.Reference.ShortMessage)
                    .Should()
                    .Equal(testRepository.Branches.Select(x => x.Reference.ShortMessage));
            }
        }
    }
}

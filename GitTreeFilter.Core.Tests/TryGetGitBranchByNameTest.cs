using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Tests.Extensions;
using GitTreeFilter.Core.Tests.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.TryGetGitBranchByName")]
    public class TryGetGitBranchByNameTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void CanResolve(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedBranch = testRepository.Branches.RandomElement();

            // when
            var wasResolved = solutionRepository.TryGetGitBranchByName(expectedBranch.FriendlyName, out var resolvedBranch);

            // then
            using (new AssertionScope())
            {
                wasResolved.Should().BeTrue();
                resolvedBranch.Should().BeEquivalentTo(expectedBranch);
            }
        }

        [TestMethod]
        public void DoesNotResolveIfNotPresent()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);

            // when
            var wasResolved = solutionRepository.TryGetGitBranchByName("branch-which-does-not-exist", out var resolvedBranch);

            // then
            using (new AssertionScope())
            {
                wasResolved.Should().BeFalse();
                resolvedBranch.Should().BeNull();
            }
        }
    }
}

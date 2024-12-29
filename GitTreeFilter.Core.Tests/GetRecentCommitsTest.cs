using FluentAssertions;
using GitTreeFilter.Core.Tests.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.RecentCommits")]
    public class GetRecentCommitsTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void IsCorrect(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var recentCommits = solutionRepository.GetRecentCommits();

            // then
            recentCommits.Should()
                .NotContainNulls()
                .And
                .OnlyHaveUniqueItems()
                .And
                .HaveCountGreaterThan(0)
                .And
                .AllSatisfy(x =>
                {
                    x.Reference.Should().NotBeNull();
                    x.FriendlyName.Should().NotBeNullOrEmpty();
                    x.ShortSha.Should().NotBeNullOrEmpty();
                });
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ShaMatch(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var recentCommits = solutionRepository.GetRecentCommits();

            // then
            recentCommits.Should()
                .Equal(testRepository.HeadCommits);
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ShortMessagesMatch(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var recentCommits = solutionRepository.GetRecentCommits();

            // then
            recentCommits
                .Select(x => x.ShortMessage)
                .Should()
                .Equal(testRepository.HeadCommits.Select(x => x.ShortMessage));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void NumberOfCommitsRequestedMatch(int expectedCommitCount)
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);

            // when
            var recentCommits = solutionRepository.GetRecentCommits(expectedCommitCount);

            // then
            recentCommits.Should().HaveCount(expectedCommitCount);
        }

        [TestMethod]
        public void ReturnsNoMoreThanMaximumCommitsAvailable()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);
            const int moreCommitsThanExist = 1000;

            // when
            var recentCommits = solutionRepository.GetRecentCommits(moreCommitsThanExist);

            // then
            recentCommits.Should().HaveCount(TestRepositories.Basic.HeadCommits.Count);
        }
    }
}

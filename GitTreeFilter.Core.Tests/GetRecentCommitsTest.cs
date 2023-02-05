using System.Linq;
using FluentAssertions;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .Equal(TestRepositories.First.Commits);
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
                .Equal(TestRepositories.First.Commits.Select(x => x.ShortMessage));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void NumberOfCommitsRequestedMatch(int expectedCommitCount)
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);

            // when
            var recentCommits = solutionRepository.GetRecentCommits(expectedCommitCount);

            // then
            recentCommits.Should().HaveCount(expectedCommitCount);
        }

        [TestMethod]
        public void ReturnsNoMoreThanMaximumCommitsAvailable()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);
            const int moreCommitsThanExist = 1000;

            // when
            var recentCommits = solutionRepository.GetRecentCommits(moreCommitsThanExist);

            // then
            recentCommits.Should().HaveCount(TestRepositories.First.Commits.Count);
        }
    }
}

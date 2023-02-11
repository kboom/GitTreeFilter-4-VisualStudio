using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.GetRecentTags")]
    public class GetRecentTagsTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void IsCorrect(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var tags = solutionRepository.GetRecentTags();

            // then
            tags.Should()
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
        public void ResolvesToCorrectTags(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);

            // when
            var tags = solutionRepository.GetRecentTags();

            // then
            using (new AssertionScope())
            {
                tags.Select(b => b.FriendlyName)
                    .Should()
                    .Equal(testRepository.Tags.Select(x => x.FriendlyName));

                tags.Select(b => b.Reference.Sha)
                    .Should()
                    .Equal(testRepository.Tags.Select(x => x.Reference.Sha));

                tags.Select(b => b.Reference.ShortMessage)
                    .Should()
                    .Equal(testRepository.Tags.Select(x => x.Reference.ShortMessage));
            }
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void NumberOfTagsRequestedMatch(int expectedCommitCount)
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);

            // when
            var recentTags = solutionRepository.GetRecentTags(expectedCommitCount);

            // then
            recentTags.Should().HaveCount(expectedCommitCount);
        }

        [TestMethod]
        public void ReturnsNoMoreThanMaximumTagsAvailable()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);
            const int moreTagsThanExist = 1000;

            // when
            var recentTags = solutionRepository.GetRecentTags(moreTagsThanExist);

            // then
            recentTags.Should().HaveCount(TestRepositories.First.Commits.Count);
        }
    }
}

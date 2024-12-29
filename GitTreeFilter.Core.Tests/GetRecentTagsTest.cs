using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Tests.Test.Repositories;
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
                tags.Should().HaveCount(testRepository.Tags.Count);

                tags.Should().BeEquivalentTo(testRepository.Tags, options => options
                    .Including(branch => branch.FriendlyName)
                    .Including(branch => branch.Reference.Sha)
                    .Including(branch => branch.Reference.ShortMessage)
                    .RespectingRuntimeTypes());
            }
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void NumberOfTagsRequestedMatch(int expectedTagCount)
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);

            // when
            var recentTags = solutionRepository.GetRecentTags(expectedTagCount);

            // then
            recentTags.Should().HaveCount(expectedTagCount);
        }

        [TestMethod]
        public void ReturnsNoMoreThanMaximumTagsAvailable()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);
            const int moreTagsThanExist = 1000;

            // when
            var recentTags = solutionRepository.GetRecentTags(moreTagsThanExist);

            // then
            recentTags.Should().HaveCount(TestRepositories.Basic.Tags.Count);
        }
    }
}

using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using LibGit2Sharp;
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
                tags.Should().SatisfyRespectively(testRepository.Tags.Select((expectedTag, index) => new Action<GitTag>(tag =>
                {
                    using (new AssertionScope($"Evaluating expected tag '{expectedTag.FriendlyName}' at index {index}"))
                    {
                        tag.FriendlyName.Should().Be(expectedTag.FriendlyName);
                        tag.Reference.Sha.Should().Be(expectedTag.Reference.Sha);
                        tag.Reference.ShortMessage.Should().Be(expectedTag.Reference.ShortMessage);
                    }
                })));
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

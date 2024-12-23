using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
                branches.Should().SatisfyRespectively(testRepository.Branches.Select((expectedBranch, index) => new Action<GitBranch>(branch =>
                {
                    using (new AssertionScope($"Evaluating expected branch '{expectedBranch.FriendlyName}' at index {index}"))
                    {
                        branch.FriendlyName.Should().Be(expectedBranch.FriendlyName);
                        branch.Reference.Sha.Should().Be(expectedBranch.Reference.Sha, because: $"Branch {expectedBranch.FriendlyName} should have given SHA");
                        branch.Reference.ShortMessage.Should().Be(expectedBranch.Reference.ShortMessage, because: $"Branch {expectedBranch.FriendlyName} should have given short message");
                    }
                })));
            }
        }
    }
}

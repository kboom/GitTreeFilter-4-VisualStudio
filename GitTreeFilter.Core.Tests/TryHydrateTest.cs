using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Extensions;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.TryHydrate")]
    public class TryHydrateTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void ResolvesCommitToCommit(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedCommit = testRepository.Commits.RandomElement();
            var rawCommit = new GitCommitObject(expectedCommit.Reference.Sha);
            var rawGitCommit = new GitCommit(rawCommit);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitCommit, out var rehydratedReference);

            // then
            using(new AssertionScope())
            {
                hydrated.Should().BeTrue();

                rehydratedReference.Should()
                    .NotBeNull()
                    .And
                    .BeAssignableTo<GitCommit>()
                    .And
                    .Be(expectedCommit);
            }  
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ResolvesBranchToBranch(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedBranch = testRepository.Branches.RandomElement();
            var rawCommit = new GitCommitObject(expectedBranch.Reference.Sha);
            var rawGitBranch = new GitBranch(rawCommit, expectedBranch.FriendlyName);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitBranch, out var rehydratedReference);

            // then
            using (new AssertionScope())
            {
                hydrated.Should().BeTrue();

                rehydratedReference.Should()
                    .NotBeNull()
                    .And
                    .BeAssignableTo<GitBranch>()
                    .And
                    .Be(expectedBranch);
            }
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void FastForwardsCommitsForResolvableBranches(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedBranch = testRepository.Branches[0];
            var rawCommit = new GitCommitObject(testRepository.CommitObjects.LastOrDefault().Sha);
            var rawGitBranch = new GitBranch(rawCommit, expectedBranch.FriendlyName);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitBranch, out var rehydratedReference);

            // then
            using (new AssertionScope())
            {
                hydrated.Should().BeTrue();

                rehydratedReference.Should()
                    .NotBeNull()
                    .And
                    .BeAssignableTo<GitBranch>()
                    .And
                    .Be(expectedBranch);
            }
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ResolvesBranchToCommit_IfBranchIsMissing(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedCommit = testRepository.Commits.RandomElement();
            var rawCommit = new GitCommitObject(expectedCommit.Reference.Sha);
            const string missingBranchName = "branch-that-does-not-exist";
            var rawGitBranch = new GitBranch(rawCommit, missingBranchName);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitBranch, out var rehydratedReference);

            // then
            using (new AssertionScope())
            {
                hydrated.Should().BeTrue();

                rehydratedReference.Should()
                    .NotBeNull()
                    .And
                    .BeAssignableTo<GitCommit>()
                    .And
                    .Be(expectedCommit);
            }
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ResolvesTagToCommit_IfTagIsMissing(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            var expectedCommit = testRepository.Commits.RandomElement();
            var rawCommit = new GitCommitObject(expectedCommit.Reference.Sha);
            const string missingTagName = "branch-that-does-not-exist";
            var rawGitTag = new GitTag(rawCommit, missingTagName);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitTag, out var rehydratedReference);

            // then
            using (new AssertionScope())
            {
                hydrated.Should().BeTrue();

                rehydratedReference.Should()
                    .NotBeNull()
                    .And
                    .BeAssignableTo<GitCommit>()
                    .And
                    .Be(expectedCommit);
            }
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void IsFalseWhenCommitDoesNotExist(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            const string notExistentSha = "d670460b4b4aece5915caf5c68d12f560a9fe3e4";
            var rawCommit = new GitCommitObject(notExistentSha);
            var rawGitCommit = new GitCommit(rawCommit);

            // when
            var hydrated = solutionRepository.TryHydrate(rawGitCommit, out var rehydratedReference);

            // then
            using (new AssertionScope())
            {
                hydrated.Should().BeFalse();
                rehydratedReference.Should().BeNull();
            }
        }
    }
}

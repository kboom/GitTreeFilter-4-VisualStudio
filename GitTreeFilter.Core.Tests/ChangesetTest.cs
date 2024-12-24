using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test;
using GitTreeFilter.Core.Tests.Test.Repositories;
using GitTreeFilter.Core.Tests.Test.Repositories.Basic;
using GitTreeFilter.Core.Tests.Test.Repositories.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("SolutionRepository.Changeset")]
    public class ChangesetTest : SolutionRepositoryTest
    {
        [TestMethod]
        public void ThrowsNothingToCompareExceptionIfReferenceNotSet()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);
            ReferenceObject = null;

            // when & then
            solutionRepository.Invoking(x => x.Changeset)
                .Should()
                .Throw<NothingToCompareException>()
                .WithMessage("Git reference was null");
        }

        [TestMethod]
        public void ThrowsRepositoryTargetNotFoundExceptionIfReferenceSetToMissingObject()
        {
            // given
            var solutionRepository = CreateSolutionRepository(TestRepositories.Basic);
            const string missingSha = "9eabf5b536662000f79978c4d1b6e4eff5c8d785";
            ReferenceObject = new GitCommit(new GitCommitObject(missingSha, "any description"));

            // when & then
            solutionRepository.Invoking(x => x.Changeset)
                .Should()
                .Throw<RepositoryTargetNotFoundException>()
                .WithMessage("Selected reference does not exist in repository");
        }

        [DataTestMethod]
        [AllRepositoriesData]
        public void ReturnsEmptyChangesetIfTargetEqualsHead(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            ReferenceObject = testRepository.Head;

            // when
            var changeset = solutionRepository.Changeset;

            // then
            using (new AssertionScope())
            {
                changeset.Should().NotBeNull();
                changeset.Should().BeEmpty();
            }
        }

        [DataTestMethod]
        [BasicRepositoryChangesetDataSource]
        public void BasicRepositoryChangeset(
            ITestRepository testRepository,
            TestComparisonConfig comparisonConfig,
            IImmutableSet<string> changedFilesPaths)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            SetComparisonConfig(comparisonConfig);

            // when
            var changeset = solutionRepository.Changeset;

            // then
            using (new AssertionScope())
            {
                changeset.Should().NotBeNull();
                changeset.Select(x => x.AbsoluteFilePath).Should().BeEquivalentTo(changedFilesPaths);
            }
        }

        [DataTestMethod]
        [MergeRepositoryChangesetDataSource]
        public void MergeRepositoryChangeset(
           ITestRepository testRepository,
           TestComparisonConfig comparisonConfig,
           IImmutableSet<string> changedFilesPaths)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            SetComparisonConfig(comparisonConfig);

            // when
            var changeset = solutionRepository.Changeset;

            // then
            using (new AssertionScope())
            {
                changeset.Should().NotBeNull();
                changeset.Select(x => x.AbsoluteFilePath).Should().BeEquivalentTo(changedFilesPaths);
            }
        }
    }
}

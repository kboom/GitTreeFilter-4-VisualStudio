using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Exceptions;
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
    public class DetachedHeadChangesetTest : SolutionRepositoryTest
    {
        [DataTestMethod]
        [AllRepositoriesData]
        public void DoesNotThrowWhenHeadIsDetached(ITestRepository testRepository)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            ReferenceObject = testRepository.Head;

            // when & then
            RepositoryState.WithDetachedHead(testRepository, () =>
            {
                solutionRepository.Invoking(x => x.Changeset)
                    .Should()
                    .NotThrow<HeadNotFoundException>();
            });
        }

        [DataTestMethod]
        [BasicRepositoryChangesetDataSource]
        public void BasicRepositoryChangesetWithDetachedHead(
            ITestRepository testRepository,
            TestComparisonConfig comparisonConfig,
            IImmutableSet<string> changedFilesPaths)
        {
            AssertChangesetUnderDetachedHead(testRepository, comparisonConfig, changedFilesPaths);
        }

        [DataTestMethod]
        [MergeRepositoryChangesetDataSource]
        public void MergeRepositoryChangesetWithDetachedHead(
            ITestRepository testRepository,
            TestComparisonConfig comparisonConfig,
            IImmutableSet<string> changedFilesPaths)
        {
            AssertChangesetUnderDetachedHead(testRepository, comparisonConfig, changedFilesPaths);
        }

        private void AssertChangesetUnderDetachedHead(
            ITestRepository testRepository,
            TestComparisonConfig comparisonConfig,
            IImmutableSet<string> changedFilesPaths)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            SetComparisonConfig(comparisonConfig);

            // when & then
            RepositoryState.WithDetachedHead(testRepository, () =>
            {
                var changeset = solutionRepository.Changeset;

                using (new AssertionScope())
                {
                    changeset.Should().NotBeNull();
                    changeset.Select(x => x.AbsoluteFilePath).Should().BeEquivalentTo(changedFilesPaths);
                }
            });
        }
    }
}

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.DataSource;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);
            solutionRepository.GitReference = null;

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
            var solutionRepository = CreateSolutionRepository(TestRepositories.First);
            const string missingSha = "9eabf5b536662000f79978c4d1b6e4eff5c8d785";
            solutionRepository.GitReference = new GitCommit(new GitCommitObject(missingSha, "any description"));

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
            solutionRepository.GitReference = testRepository.Head;

            // when
            var changeset = solutionRepository.Changeset;

            // then
            using(new AssertionScope())
            {
                changeset.Should().NotBeNull();
                changeset.Should().BeEmpty();
            } 
        }

        [DataTestMethod]
        [FirstRepositoryChangesetDataSource]
        public void FirstRepositoryChangeset(ITestRepository testRepository, GitReference<GitCommitObject> gitReference, IImmutableSet<string> changedFilesPaths)
        {
            // given
            var solutionRepository = CreateSolutionRepository(testRepository);
            solutionRepository.GitReference = gitReference;

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

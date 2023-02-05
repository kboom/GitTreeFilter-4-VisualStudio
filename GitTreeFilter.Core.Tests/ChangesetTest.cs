using FluentAssertions;
using GitTreeFilter.Core.Exceptions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
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
    }
}

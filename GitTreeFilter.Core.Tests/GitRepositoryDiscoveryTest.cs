using System;
using System.IO;
using FluentAssertions;
using GitTreeFilter.Core;
using GitTreeFilter.Core.Tests.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    [TestCategory("GitRepositoryDiscovery")]
    public class GitRepositoryDiscoveryTest
    {
        [TestMethod]
        public void DiscoversRepositoryRootWhenGitIsAFile()
        {
            // given a working directory whose ".git" is a file (submodule/worktree pointer)
            var repositoryRoot = TestRepositories.Basic.Solution.SolutionPath;
            File.Exists(Path.Combine(repositoryRoot, ".git"))
                .Should().BeTrue("this fixture must reproduce the worktree/submodule '.git is a file' case");

            // when
            var discovered = GitRepositoryDiscovery.TryGetRepositoryRoot(repositoryRoot, out var root);

            // then
            discovered.Should().BeTrue();
            NormalizeDirectory(root).Should().Be(NormalizeDirectory(repositoryRoot));
        }

        [TestMethod]
        public void DiscoversRepositoryRootFromNestedSubdirectory()
        {
            // given a nested subdirectory inside the repository working directory
            var repositoryRoot = TestRepositories.Basic.Solution.SolutionPath;
            var nested = Path.Combine(repositoryRoot, "nested-" + Guid.NewGuid().ToString("N"), "deeper");
            Directory.CreateDirectory(nested);

            try
            {
                // when
                var discovered = GitRepositoryDiscovery.TryGetRepositoryRoot(nested, out var root);

                // then
                discovered.Should().BeTrue();
                NormalizeDirectory(root).Should().Be(NormalizeDirectory(repositoryRoot));
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(nested), recursive: true);
            }
        }

        [TestMethod]
        public void ReturnsFalseWhenPathIsNotInsideRepository()
        {
            // given a directory that is not inside any git repository
            var outside = Path.Combine(Path.GetTempPath(), "gtf-not-a-repo-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outside);

            try
            {
                // when
                var discovered = GitRepositoryDiscovery.TryGetRepositoryRoot(outside, out var root);

                // then
                discovered.Should().BeFalse();
                root.Should().BeNull();
            }
            finally
            {
                Directory.Delete(outside, recursive: true);
            }
        }

        private static string NormalizeDirectory(string path) =>
            Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

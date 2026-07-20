using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test;
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    /// <summary>
    /// Regression tests for repositories that Visual Studio has already opened but
    /// whose working directory is owned by a different account than the one running
    /// Visual Studio (git worktrees or clones created by another user - common on
    /// enterprise machines). libgit2 &gt;= 1.4 refuses to open such repositories
    /// ("repository path ... is not owned by current user", CVE-2022-24765). That
    /// exception was thrown while the plugin was setting up, which aborted service
    /// creation and left the filter provider with a null service - so selecting the
    /// filter did nothing ("cannot open").
    /// </summary>
    [TestClass]
    [TestCategory("SolutionRepository")]
    public class RepositoryOwnershipTest
    {
        [TestMethod]
        public void OpeningRepositoryDisablesLibGit2OwnerValidation()
        {
            if (!IsGitAvailable())
            {
                Assert.Inconclusive("git CLI is required to materialize a repository for this test.");
            }

            // baseline: libgit2 owner validation on (its default) - this is what
            // rejects repositories owned by another user.
            GlobalSettings.SetOwnerValidation(true);

            var repositoryPath = CreateRepository();

            try
            {
                var solutionRepository = SolutionRepositoryFactory.CreateSolutionRepository(
                    new GitSolution(repositoryPath), new TestComparisonConfig());

                // when: the plugin opens the repository through its factory.
                _ = solutionRepository.Branches.ToList();

                // then: owner validation must have been disabled so repositories that
                // Visual Studio already trusts (e.g. another user's worktree) can open.
                GlobalSettings.GetOwnerValidation().Should().BeFalse(
                    "the plugin must trust repositories Visual Studio has already opened");
            }
            finally
            {
                ForceDeleteDirectory(repositoryPath);
                GlobalSettings.SetOwnerValidation(true);
            }
        }

        private static string CreateRepository()
        {
            var path = Path.Combine(Path.GetTempPath(), "gtf-ownership-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);

            Git(path, "init");
            Git(path, "config core.autocrlf false");
            Git(path, "config user.email test@example.com");
            Git(path, "config user.name test");

            File.WriteAllText(Path.Combine(path, "file.txt"), "content");
            Git(path, "add .");
            Git(path, "commit -m init");

            return path;
        }

        private static bool IsGitAvailable()
        {
            try
            {
                RunProcess(Environment.CurrentDirectory, "git", "--version");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Git(string workingDirectory, string arguments) =>
            RunProcess(workingDirectory, "git", arguments);

        private static string RunProcess(string workingDirectory, string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'{fileName} {arguments}' failed with exit code {process.ExitCode}: {error}");
            }

            return output;
        }

        private static void ForceDeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
                catch
                {
                    // best effort
                }
            }

            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch
            {
                // best effort cleanup of a temp directory
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests
{
    /// <summary>
    /// Regression tests for the "invalid data in index - calculated checksum does not
    /// match expected" failure. Modern git (2.x with feature.manyFiles / untracked cache)
    /// writes index extensions that the previously bundled libgit2 could not parse, which
    /// made <see cref="SolutionRepository.Changeset"/> throw and left the Solution Explorer
    /// list empty ("won't budge").
    /// </summary>
    [TestClass]
    [TestCategory("SolutionRepository.Changeset")]
    public class ModernIndexChangesetTest
    {
        [TestMethod]
        public void ChangesetDoesNotThrowForModernGitIndex()
        {
            // given a repository whose on-disk index was written by a modern git with
            // feature.manyFiles enabled (index v4 + untracked-cache extension). This is the
            // corner case that older libgit2 rejected with a checksum error.
            if (!IsGitAvailable())
            {
                Assert.Inconclusive("git CLI is required to materialize a modern index for this test.");
            }

            var repositoryPath = CreateModernIndexRepository(out var headSha);

            try
            {
                var comparisonConfig = new TestComparisonConfig
                {
                    ReferenceObject = new GitCommit(new GitCommitObject(headSha, "HEAD"))
                };

                var solutionRepository = SolutionRepositoryFactory.CreateSolutionRepository(
                    new GitSolution(repositoryPath), comparisonConfig);

                // when & then: reading the changeset must parse the index without throwing.
                solutionRepository.Invoking(x => x.Changeset)
                    .Should()
                    .NotThrow("modern git index extensions must be parseable");

                solutionRepository.Changeset.Should().NotBeNull();
            }
            finally
            {
                ForceDeleteDirectory(repositoryPath);
            }
        }

        /// <summary>
        /// Builds a throwaway git repository whose index is large enough and configured such
        /// that git writes the modern extensions (index v4 + untracked cache) that reproduce
        /// the checksum-parsing failure. Returns the working directory path.
        /// </summary>
        private static string CreateModernIndexRepository(out string headSha)
        {
            var path = Path.Combine(Path.GetTempPath(), "gtf-modern-index-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);

            Git(path, "init");
            // feature.manyFiles turns on index.version=4, index.threads and core.untrackedCache,
            // i.e. exactly the extensions that older libgit2 mis-parsed.
            Git(path, "config feature.manyFiles true");
            Git(path, "config core.autocrlf false");
            Git(path, "config user.email test@example.com");
            Git(path, "config user.name test");

            // Enough files (spread across directories) to push the index past the size at which
            // the extension mis-parse manifests as a checksum mismatch.
            for (var i = 0; i < 500; i++)
            {
                var directory = Path.Combine(path, "dir" + (i % 16));
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "f" + i + ".txt"), "content " + i);
            }

            Git(path, "add .");
            Git(path, "commit -m init");
            // Force git to (re)write the untracked-cache extension into the index.
            Git(path, "status --short");

            headSha = Git(path, "rev-parse HEAD").Trim();
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

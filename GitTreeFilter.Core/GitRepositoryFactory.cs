using LibGit2Sharp;
using System.IO;

namespace GitTreeFilter.Core
{
    internal interface IGitRepositoryFactory
    {
        IRepository Create(GitSolution solution);
    }

    internal class GitRepositoryFactory
    {
        public IRepository Create(GitSolution solution)
        {
            var currentDirectory = solution.SolutionPath;
            try
            {
                while (!Directory.Exists(Path.Combine(currentDirectory, ".git")) && !File.Exists(Path.Combine(currentDirectory, ".git")) && Path.GetPathRoot(currentDirectory) != currentDirectory)
                {
                    currentDirectory = Path.GetDirectoryName(currentDirectory);
                }
                return new Repository(currentDirectory);
            }
            catch (RepositoryNotFoundException)
            {
                throw new GitRepoNotFoundException($"Unable to find a Git repository at this solution's directory ({solution.SolutionPath}) or it's parent directories.");
            }
        }
    }
}

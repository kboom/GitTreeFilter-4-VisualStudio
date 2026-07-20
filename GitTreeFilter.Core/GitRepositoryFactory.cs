using LibGit2Sharp;
using System.IO;

namespace GitTreeFilter.Core;

internal class GitRepositoryFactory
{
    public IRepository Create(GitSolution solution)
    {
        // libgit2 (>= 1.4) refuses to open a repository whose working directory is
        // owned by a different account than the current process ("repository path ...
        // is not owned by current user", CVE-2022-24765). Visual Studio has already
        // opened this repository for the user and the plugin only reads it, so trust
        // it. Without this, worktrees or clones created by another account (common on
        // enterprise machines) throw during setup and deactivate the plugin entirely.
        GlobalSettings.SetOwnerValidation(false);

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

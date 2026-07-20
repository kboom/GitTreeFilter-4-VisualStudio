using System.IO;
using LibGit2Sharp;

namespace GitTreeFilter.Core;

/// <summary>
/// Filesystem based discovery of the Git repository that owns a given path.
/// Unlike Visual Studio's <c>IGitExt.ActiveRepositories</c>, this works for
/// repositories that Visual Studio does not surface - notably git worktrees and
/// submodules (whose ".git" is a file pointing elsewhere) - as well as when the
/// solution lives in a nested subdirectory of the repository.
/// </summary>
public static class GitRepositoryDiscovery
{
    /// <summary>
    /// Walks up from <paramref name="startingPath"/> to locate the owning Git
    /// repository and returns its working directory.
    /// </summary>
    /// <returns>
    /// <c>true</c> and the repository working directory in
    /// <paramref name="repositoryRoot"/> when a repository is found; otherwise
    /// <c>false</c> and <c>null</c>.
    /// </returns>
    public static bool TryGetRepositoryRoot(string startingPath, out string repositoryRoot)
    {
        repositoryRoot = null;

        if (string.IsNullOrEmpty(startingPath))
        {
            return false;
        }

        // Repository.Discover needs a directory path; if a file was supplied, use its directory.
        var searchPath = File.Exists(startingPath) ? Path.GetDirectoryName(startingPath) : startingPath;
        if (string.IsNullOrEmpty(searchPath) || !Directory.Exists(searchPath))
        {
            return false;
        }

        // Repository.Discover resolves worktree/submodule ".git" pointer files and
        // walks up parent directories, returning the path to the resolved git directory.
        var gitDirectory = Repository.Discover(searchPath);
        if (string.IsNullOrEmpty(gitDirectory))
        {
            return false;
        }

        using var repository = new Repository(gitDirectory);
        var workingDirectory = repository.Info.WorkingDirectory;
        if (string.IsNullOrEmpty(workingDirectory))
        {
            // Bare repository - there is no working tree to filter.
            return false;
        }

        repositoryRoot = Path.GetFullPath(workingDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return true;
    }
}

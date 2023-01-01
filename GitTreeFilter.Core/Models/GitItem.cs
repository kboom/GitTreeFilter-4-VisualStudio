using System;
using System.IO;
using System.Text;
using LibGit2Sharp;

namespace GitTreeFilter.Core.Models
{
    public class GitItem
    {
        private readonly GitSolution _solution;
        private readonly GitRepositoryFactory _gitRepositoryFactory;

        internal GitItem(
            GitSolution solution,
            GitRepositoryFactory gitRepositoryFactory,
            GitReference<GitCommitObject> gitReference,
            string absoluteFilePath,
            string oldAbsoluteFilePath = null)
        {
            _solution = solution;
            _gitRepositoryFactory = gitRepositoryFactory;
            GitReference = gitReference;
            AbsoluteFilePath = absoluteFilePath;
            OldAbsoluteFilePath = oldAbsoluteFilePath;
        }

        public GitReference<GitCommitObject> GitReference { get; }

        public string AbsoluteFilePath { get; }

        public string OldAbsoluteFilePath { get; }

        public override int GetHashCode() => AbsoluteFilePath.GetHashCode();


        public string BaseBranchRevisionOfFile
        {
            get
            {
                using (var repository = _gitRepositoryFactory.Create(_solution))
                {
                    var usedAbsolutePath = string.IsNullOrEmpty(AbsoluteFilePath) ? OldAbsoluteFilePath : AbsoluteFilePath;
                    var workingDirectory = repository.Info.WorkingDirectory;
                    var relativePathInRepo = GetRelativePath(usedAbsolutePath, workingDirectory);
                    var treeEntry = GetTreeEntryAtPath(repository, relativePathInRepo);
                    var tempFile = CreateFilename(usedAbsolutePath);
                    var fileEncoding = GetEncoding(usedAbsolutePath);

                    if (TryGetBlob(treeEntry, out var blob))
                    {
                        var fileContents = blob.GetContentText(new FilteringOptions(relativePathInRepo));
                        File.WriteAllText(tempFile, fileContents, fileEncoding);
                    }
                    else
                    {
                        File.WriteAllText(tempFile, string.Empty, fileEncoding);
                    } 

                    return tempFile;
                }
            }
        }

        private string CreateFilename(string usedAbsolutePath) =>
            Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}{Path.GetExtension(usedAbsolutePath)}");

        private static bool TryGetBlob(TreeEntry treeEntry, out Blob blob)
        {
            blob = default;
            if (treeEntry == null)
            {
                return false;
            }

            if (treeEntry.TargetType is TreeEntryTargetType.Blob)
            {
                var treeEntryblob = treeEntry.Target as Blob;
                if (!treeEntryblob.IsBinary)
                {
                    blob = treeEntryblob;
                    return true;
                }
            }

            return false;
        }

        private TreeEntry GetTreeEntryAtPath(IRepository repository, string relativePathInRepo)
        {
            var targetCommit = repository.GetTargetCommit(GitReference);
            var relativeUnescapedPathInRepo = relativePathInRepo.Replace(Path.DirectorySeparatorChar, '/');
            var treeEntryAtTipOfBase = targetCommit[relativeUnescapedPathInRepo];
            return treeEntryAtTipOfBase;
        }

        private static string GetRelativePath(string usedAbsolutePath, string workingDirectory)
        {
            if (usedAbsolutePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return usedAbsolutePath.Substring(workingDirectory.Length);
            }
            else
            {
                return usedAbsolutePath;
            } 
        }

        private static Encoding GetEncoding(string file)
        {
            if (File.Exists(file))
            {
                var encoding = Encoding.UTF8;
                if (HasPreamble(file, encoding))
                {
                    return encoding;
                }
            }

            return Encoding.Default;
        }

        private static bool HasPreamble(string file, Encoding encoding)
        {
            using (var stream = File.OpenRead(file))
            {
                foreach (var b in encoding.GetPreamble())
                {
                    if (b != stream.ReadByte())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is GitItem diffResultItem)
            {
                return GetHashCode() == diffResultItem.GetHashCode();
            }

            return false;
        }
    }
}
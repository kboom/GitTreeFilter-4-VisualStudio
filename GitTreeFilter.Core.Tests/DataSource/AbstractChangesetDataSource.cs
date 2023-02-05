using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests.DataSource
{
    [AttributeUsage(AttributeTargets.Method)]
    internal abstract class AbstractChangesetDataSource : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            foreach (var descriptor in GetDescriptors())
            {
                var repository = descriptor.Repository;
                var basePathForFiles = repository.Solution.SolutionPath;
                var files = descriptor.FilesInChangeset
                    .Select(x => Path.Combine(basePathForFiles, x))
                    .ToImmutableHashSet();

                yield return new object[] { repository, descriptor.Reference, files };
            }
        }

        public abstract IReadOnlyList<ChangesetDescriptor> GetDescriptors();

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
            {
                var repositoryName = (data[0] as ITestRepository).Name;
                var targetName = (data[1] as GitReference<GitCommitObject>).FriendlyName;
                var targetType = data[1].GetType();

                return string.Format(CultureInfo.CurrentCulture, "{0} ({1} reference {2} {3})", methodInfo.Name, repositoryName, targetName, targetType);
            }

            return null;
        }
    }

    internal class ChangesetDescriptor
    {
        public ITestRepository Repository { get; set; }

        public GitReference<GitCommitObject> Reference { get; set; }

        public IReadOnlyList<string> FilesInChangeset { get; set; }
    }
}

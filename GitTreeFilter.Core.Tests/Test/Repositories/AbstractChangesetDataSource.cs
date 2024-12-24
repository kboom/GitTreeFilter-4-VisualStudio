using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GitTreeFilter.Core.Tests.Test.Repositories
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

                yield return new object[] { repository, descriptor.ComparisonConfig, files };
            }
        }

        public abstract IReadOnlyList<ChangesetDescriptor> GetDescriptors();

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
            {
                TestComparisonConfig config = data[1] as TestComparisonConfig;
                var repositoryName = (data[0] as ITestRepository).Name;
                var targetName = config.ReferenceObject.FriendlyName;
                var targetType = config.ReferenceObject.GetType().Name;

                return string.Format(CultureInfo.CurrentCulture, "{4}: {0} ({3} {2} in {1})", methodInfo.Name, repositoryName, targetName, targetType, config.TestName);
            }

            return null;
        }
    }

    internal class ChangesetDescriptor
    {
        public string TestName { get; set; }

        public ITestRepository Repository { get; set; }

        public IReadOnlyList<string> FilesInChangeset { get; set; }

        public TestComparisonConfig ComparisonConfig { get; set; }
    }
}

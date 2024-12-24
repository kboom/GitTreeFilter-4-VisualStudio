using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace GitTreeFilter.Core.Tests.Test.Repositories
{
    [AttributeUsage(AttributeTargets.Method)]
    internal abstract class AbstractRepositoryDataSource : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            foreach (var testRepository in GetTestRepositories())
            {
                yield return new object[] { testRepository };
            }
        }

        public abstract IReadOnlyList<ITestRepository> GetTestRepositories();

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
                return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", methodInfo.Name, (data[0] as ITestRepository).Name);

            return null;
        }
    }
}

using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test;
using GitTreeFilter.Core.Tests.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GitTreeFilter.Core.Tests
{
    [TestClass]
    public abstract class SolutionRepositoryTest : IComparisonConfig
    {
        protected ISolutionRepository CreateSolutionRepository(ITestRepository testRepository)
        {
            return SolutionRepositoryFactory.CreateSolutionRepository(testRepository.Solution, this);
        }

        public GitReference<GitCommitObject> ReferenceObject
        {
            get { return _comparisonConfig.ReferenceObject; }
            set { _comparisonConfig.ReferenceObject = value; }
        }

        public bool OriginRefsOnly
        {
            get { return _comparisonConfig.OriginRefsOnly; }
            set { _comparisonConfig.OriginRefsOnly = value; }
        }

        public bool IncludeUnstagedChanges
        {
            get { return _comparisonConfig.IncludeUnstagedChanges; }
            set { _comparisonConfig.IncludeUnstagedChanges = value; }
        }

        internal void SetComparisonConfig(TestComparisonConfig comparisonConfig)
        {
            if (comparisonConfig is null)
            {
                throw new ArgumentNullException(nameof(comparisonConfig));
            }
            _comparisonConfig = comparisonConfig;
        }

        private TestComparisonConfig _comparisonConfig = new();
    }
}
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Repositories;
using GitTreeFilter.Core.Tests.Test;
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

        public bool OriginRefsOnly { 
            get { return _comparisonConfig.OriginRefsOnly; }
            set { _comparisonConfig.OriginRefsOnly = value; }
        }

        public bool PinToMergeHead
        {
            get { return _comparisonConfig.PinToMergeHead; }
            set { _comparisonConfig.PinToMergeHead = value; }
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
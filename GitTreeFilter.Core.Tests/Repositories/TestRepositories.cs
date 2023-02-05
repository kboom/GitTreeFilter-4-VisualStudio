using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTreeFilter.Core.Tests.Repositories
{
    public static class TestRepositories
    {
        public static readonly ITestRepository First = new BasicTestRepository();
        public static readonly ITestRepository DetachedHead = new DetachedHeadRepository();
    }
}

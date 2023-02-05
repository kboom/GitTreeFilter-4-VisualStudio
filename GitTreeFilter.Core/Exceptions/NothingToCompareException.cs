using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTreeFilter.Core.Exceptions
{
    public sealed class NothingToCompareException : GitOperationException
    {
        public NothingToCompareException() : base("Git reference was null") { }
    }
}

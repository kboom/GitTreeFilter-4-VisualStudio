﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTreeFilter.Core.Exceptions
{
    public sealed class RepositoryTargetNotFoundException : GitOperationException
    {
        public RepositoryTargetNotFoundException() : base("Selected reference does not exist in repository") { }
    }
}

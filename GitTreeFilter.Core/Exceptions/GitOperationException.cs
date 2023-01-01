using System;

namespace GitTreeFilter.Core
{
    public abstract class GitOperationException : InvalidOperationException
    {

        public GitOperationException() { }
        public GitOperationException(string message): base(message) { }
    }
}

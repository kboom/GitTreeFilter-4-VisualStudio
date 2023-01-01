namespace GitTreeFilter.Core
{
    public class GitRepoNotFoundException : GitOperationException
    {
        public GitRepoNotFoundException(string message) : base(message)
        {
        }
    }
}

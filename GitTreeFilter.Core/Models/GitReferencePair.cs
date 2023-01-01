namespace GitTreeFilter.Core.Models
{
    public class GitReferencePair<T> 
        where T : GitReference<GitCommitObject>
    {
        private readonly GitBranch _working;
        private readonly T _foreign;

        public GitReferencePair(GitBranch working, T foreign)
        {
            _working = working;
            _foreign = foreign;
        }

        public GitBranch Working => _working;

        public T Target => _foreign;
    }
}

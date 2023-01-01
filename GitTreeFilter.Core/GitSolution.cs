namespace GitTreeFilter.Core
{
    public class GitSolution
    {
        private readonly string _solutionPath;

        public GitSolution(string solutionPath) => _solutionPath = solutionPath;

        public string SolutionPath => _solutionPath;
    }
}

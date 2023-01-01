using Microsoft.Extensions.DependencyInjection;

namespace GitTreeFilter.Core
{
    public class SolutionRepositoryFactory
    {
        public static ISolutionRepository CreateSolutionRepository(GitSolution solution, IComparisonConfig comparisonConfig)
        {
            return new SolutionRepository(
                solution,
                comparisonConfig,
        ServiceContext.Instance.GetRequiredService<GitRepositoryFactory>()
            );
        }
    }
}

using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitCommitObjectFaker
{
    private string Sha { get; set; }

    public GitCommitObjectFaker()
    {
    }

    public GitCommitObjectFaker(GitCommitObjectFaker gitCommitObjectFaker)
    {
        Sha = gitCommitObjectFaker.Sha;
    }

    public GitCommitObjectFaker WithRandomSha()
    {
        return new GitCommitObjectFaker(this)
        {
            Sha = FakeHelpers.GenerateRandomSha()
        };
    }

    public GitCommitObject Create()
    {
        return new GitCommitObject(Sha);
    }
}

using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitObjectFaker
{
    public GitObjectFaker()
    {

    }

    public GitObjectFaker(GitObjectFaker gitObjectFaker)
    {
        Sha = gitObjectFaker.Sha;
    }

    private string Sha { get; set; }

    public GitObjectFaker WithRandomSha()
    {
        return new GitObjectFaker(this)
        {
            Sha = FakeHelpers.GenerateRandomSha()
        };
    }

    public GitObject Create()
    {
        return new GitObject(Sha);
    }
}

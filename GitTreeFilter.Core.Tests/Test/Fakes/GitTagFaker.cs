using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitTagFaker
{
    private string Sha { get; set; }
    private string Name { get; set; }

    public GitTagFaker()
    {
    }

    public GitTagFaker(GitTagFaker gitTagFaker)
    {
        Sha = gitTagFaker.Sha;
        Name = gitTagFaker.Name;
    }

    public GitTagFaker WithRandomSha()
    {
        return new GitTagFaker(this)
        {
            Sha = FakeHelpers.GenerateRandomSha()
        };
    }

    public GitTagFaker WithName(string name)
    {
        return new GitTagFaker(this)
        {
            Name = name
        };
    }

    public GitTag Create()
    {
        var gitCommitObject = new GitCommitObject(Sha);
        return new GitTag(gitCommitObject, Name);
    }
}

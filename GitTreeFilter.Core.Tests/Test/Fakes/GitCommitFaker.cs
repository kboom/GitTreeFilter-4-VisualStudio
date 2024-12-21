using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitCommitFaker
{
    private string Sha { get; set; }
    private string ShortMessage { get; set; }

    public GitCommitFaker()
    {
    }

    public GitCommitFaker(GitCommitFaker gitCommitFaker)
    {
        Sha = gitCommitFaker.Sha;
        ShortMessage = gitCommitFaker.ShortMessage;
    }

    public GitCommitFaker WithRandomSha()
    {
        return new GitCommitFaker(this)
        {
            Sha = FakeHelpers.GenerateRandomSha()
        };
    }

    public GitCommitFaker WithShortMessage(string shortMessage)
    {
        return new GitCommitFaker(this)
        {
            ShortMessage = shortMessage
        };
    }

    public GitCommit Create()
    {
        var gitCommitObject = new GitCommitObject(Sha, ShortMessage);
        return new GitCommit(gitCommitObject);
    }
}

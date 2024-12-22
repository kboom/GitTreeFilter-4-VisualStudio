using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitBranchFaker
{
    private string Sha { get; set; } = FakeHelpers.GenerateRandomSha();
    private string ShortName { get; set; } = FakeHelpers.GenerateRandomName();

    public GitBranchFaker()
    {
    }

    public GitBranchFaker(GitBranchFaker gitBranchFaker)
    {
        Sha = gitBranchFaker.Sha;
        ShortName = gitBranchFaker.ShortName;
    }

    public GitBranchFaker WithRandomSha()
    {
        return new GitBranchFaker(this)
        {
            Sha = FakeHelpers.GenerateRandomSha()
        };
    }

    public GitBranchFaker WithRandomShortName()
    {
        return new GitBranchFaker(this)
        {
            ShortName = FakeHelpers.GenerateRandomName()
        };
    }

    public GitBranch Create()
    {
        var gitCommitObject = new GitCommitObject(Sha);
        return new GitBranch(gitCommitObject, ShortName);
    }
}

using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test.Fakes;

public class GitBranchFaker
{
    private string Sha { get; set; } = FakeHelpers.GenerateRandomSha();
    private string ShortName { get; set; } = FakeHelpers.GenerateRandomName();
    private bool PinToMergeHead { get; set; }

    public GitBranchFaker()
    {
    }

    public GitBranchFaker(GitBranchFaker gitBranchFaker)
    {
        Sha = gitBranchFaker.Sha;
        ShortName = gitBranchFaker.ShortName;
        PinToMergeHead = gitBranchFaker.PinToMergeHead;
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

    public GitBranchFaker WithPinToMergeHead(bool pinToMergeHead)
    {
        return new GitBranchFaker(this)
        {
            PinToMergeHead = pinToMergeHead
        };
    }

    public GitBranch Create()
    {
        var gitCommitObject = new GitCommitObject(Sha);
        return new GitBranch(gitCommitObject, ShortName, PinToMergeHead);
    }
}

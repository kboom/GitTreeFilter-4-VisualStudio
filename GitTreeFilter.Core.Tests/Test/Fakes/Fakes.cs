namespace GitTreeFilter.Core.Tests.Test.Fakes;

public static class Fakes
{
    public static GitObjectFaker GitObject => new();

    public static GitCommitObjectFaker GitCommitObject => new();

    public static GitBranchFaker GitBranch => new();

    public static GitCommitFaker GitCommit => new();

    public static GitTagFaker GitTag => new();
}

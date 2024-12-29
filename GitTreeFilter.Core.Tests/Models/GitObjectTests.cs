using FluentAssertions;
using FluentAssertions.Execution;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Core.Tests.Test.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitTreeFilter.Core.Tests.Models;

[TestClass]
public class GitObjectTests
{
    [TestMethod]
    public void GitObject_Equality()
    {
        GitObjectFaker gitObjectFaker = Fakes.GitObject;

        GitObject firstObject = gitObjectFaker
            .WithRandomSha()
            .Create();

        GitObject secondObject = gitObjectFaker
            .WithRandomSha()
            .Create();

        using (new AssertionScope())
        {
            firstObject.Should().NotBe(secondObject);
            firstObject.Should().Be(firstObject);
            secondObject.Should().Be(secondObject);
        }
    }

    [TestMethod]
    public void GitCommitObject_Equality()
    {
        GitCommitObjectFaker gitCommitObjectFaker = Fakes.GitCommitObject;

        GitCommitObject firstObject = gitCommitObjectFaker
            .WithRandomSha()
            .Create();

        GitCommitObject secondObject = gitCommitObjectFaker
            .WithRandomSha()
            .Create();

        using (new AssertionScope())
        {
            firstObject.Should().NotBe(secondObject);
            firstObject.Should().Be(firstObject);
            secondObject.Should().Be(secondObject);
        }
    }

    [TestMethod]
    public void GitCommit_Equality()
    {
        GitCommitFaker gitCommitFaker = Fakes.GitCommit;

        GitCommit firstCommit = gitCommitFaker
            .WithRandomSha()
            .Create();

        GitCommit secondCommit = gitCommitFaker
            .WithRandomSha()
            .Create();

        using (new AssertionScope())
        {
            firstCommit.Should().NotBe(secondCommit);
            firstCommit.Should().Be(firstCommit);
            secondCommit.Should().Be(secondCommit);
        }
    }

    [TestMethod]
    public void GitBranch_Equality()
    {
        GitBranchFaker gitBranchFaker = Fakes.GitBranch;

        GitBranch firstBranch = gitBranchFaker
            .WithRandomSha()
            .Create();

        GitBranch secondBranch = gitBranchFaker
            .WithRandomSha()
            .Create();

        using (new AssertionScope())
        {
            firstBranch.Should().NotBe(secondBranch);
            firstBranch.Should().Be(firstBranch);
            secondBranch.Should().Be(secondBranch);
        }
    }

    [TestMethod]
    public void GitTag_Equality()
    {
        GitTagFaker gitTagFaker = Fakes.GitTag;

        GitTag firstTag = gitTagFaker
            .WithRandomSha()
            .Create();

        GitTag secondTag = gitTagFaker
            .WithRandomSha()
            .Create();

        using (new AssertionScope())
        {
            firstTag.Should().NotBe(secondTag);
            firstTag.Should().Be(firstTag);
            secondTag.Should().Be(secondTag);
        }
    }

    [TestMethod]
    public void GitObject_NotEqualToOtherTypesDespiteCommonSHA()
    {
        GitObject gitObject = Fakes.GitObject
            .WithRandomSha()
            .Create();

        GitBranch branch = Fakes.GitBranch.WithSha(gitObject.Sha).Create();
        GitTag tag = Fakes.GitTag.WithSha(gitObject.Sha).Create();

        using (new AssertionScope())
        {
            gitObject.Should().NotBe(branch);
            gitObject.Should().NotBe(tag);
        }
    }

    [TestMethod]
    public void GitCommit_NotEqualToOtherTypesDespiteCommonSHA()
    {
        GitCommit commit = Fakes.GitCommit.WithRandomSha().Create();

        GitBranch branch = Fakes.GitBranch.WithSha(commit.Reference.Sha).Create();
        GitTag tag = Fakes.GitTag.WithSha(branch.Reference.Sha).Create();

        using (new AssertionScope())
        {
            commit.Should().NotBe(branch);
            commit.Should().NotBe(tag);
        }
    }

    [TestMethod]
    public void GitBranch_NotEqualToOtherTypesDespiteCommonSHA()
    {
        GitBranch branch = Fakes.GitBranch.WithRandomSha().Create();

        GitCommit commit = Fakes.GitCommit.WithSha(branch.Reference.Sha).Create();
        GitTag tag = Fakes.GitTag.WithSha(branch.Reference.Sha).Create();

        using (new AssertionScope())
        {
            branch.Should().NotBe(tag);
            branch.Should().NotBe(commit);
        }
    }

    [TestMethod]
    public void GitTag_NotEqualToOtherTypesDespiteCommonSHA()
    {
        GitTag tag = Fakes.GitTag.WithRandomSha().Create();

        GitBranch branch = Fakes.GitBranch.WithSha(tag.Reference.Sha).Create();
        GitCommit commit = Fakes.GitCommit.WithSha(tag.Reference.Sha).Create();

        using (new AssertionScope())
        {
            tag.Should().NotBe(branch);
            tag.Should().NotBe(commit);
        }
    }

    [TestMethod]
    public void GitObject_Hashcode()
    {
        GitObjectFaker gitObjectFaker = Fakes.GitObject.WithRandomSha();
        GitObject firstObject = gitObjectFaker.Create();
        GitObject secondObject = gitObjectFaker.Create();

        int firstObjectHash = firstObject.GetHashCode();
        int secondObjectHash = secondObject.GetHashCode();

        using (new AssertionScope())
        {
            firstObjectHash.Should().Be(secondObjectHash);
        }
    }

    [TestMethod]
    public void GitCommitObject_Hashcode()
    {
        GitCommitObjectFaker gitCommitObjectFaker = Fakes.GitCommitObject.WithRandomSha();

        GitCommitObject firstObject = gitCommitObjectFaker.Create();
        GitCommitObject secondObject = gitCommitObjectFaker.Create();

        int firstObjectHash = firstObject.GetHashCode();
        int secondObjectHash = secondObject.GetHashCode();

        using (new AssertionScope())
        {
            firstObjectHash.Should().Be(secondObjectHash);
        }
    }

    [TestMethod]
    public void GitCommit_Hashcode()
    {
        GitCommitFaker gitCommitFaker = Fakes.GitCommit
            .WithRandomSha();

        GitCommit firstCommit = gitCommitFaker.Create();
        GitCommit secondCommit = gitCommitFaker.Create();

        int firstCommitHash = firstCommit.GetHashCode();
        int secondCommitHash = secondCommit.GetHashCode();

        using (new AssertionScope())
        {
            firstCommitHash.Should().Be(secondCommitHash);
        }
    }

    [TestMethod]
    public void GitBranch_Hashcode()
    {
        GitBranchFaker gitBranchFaker = Fakes.GitBranch
            .WithRandomSha()
            .WithRandomShortName();

        GitBranch firstBranch = gitBranchFaker.Create();
        GitBranch secondBranch = gitBranchFaker.Create();

        int firstBranchHash = firstBranch.GetHashCode();
        int secondBranchHash = secondBranch.GetHashCode();

        using (new AssertionScope())
        {
            firstBranchHash.Should().Be(secondBranchHash);
        }
    }

    [TestMethod]
    public void GitTag_Hashcode()
    {
        GitTagFaker gitTagFaker = Fakes.GitTag
                    .WithRandomSha();

        GitTag firstTag = gitTagFaker.Create();
        GitTag secondTag = gitTagFaker.Create();

        int firstTagHash = firstTag.GetHashCode();
        int secondTagHash = secondTag.GetHashCode();

        using (new AssertionScope())
        {
            firstTagHash.Should().Be(secondTagHash);
        }
    }
}

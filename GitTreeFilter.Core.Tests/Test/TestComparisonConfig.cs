using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core.Tests.Test;

public class TestComparisonConfig : IComparisonConfig
{
    public GitReference<GitCommitObject> ReferenceObject { get; set; }

    public bool OriginRefsOnly { get; set; }

    public bool IncludeUnstagedChanges { get; set; }

    public string TestName { get; set; }
}

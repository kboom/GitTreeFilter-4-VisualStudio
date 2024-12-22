using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core
{
    public interface IComparisonConfig
    {
        GitReference<GitCommitObject> ReferenceObject { get; }

        bool OriginRefsOnly { get; }

        bool PinToMergeHead { get; }
    }
}
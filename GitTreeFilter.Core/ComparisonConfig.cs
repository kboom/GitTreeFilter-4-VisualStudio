using GitTreeFilter.Core.Models;

namespace GitTreeFilter.Core;

public interface IComparisonConfig
{
    /// <summary>
    /// The <see cref="GitReference{T}"/> object to compare against.
    /// Only the next objects following the reference object will be included in the changeset.
    /// The objects included in the reference object itself are excluded.
    /// </summary>
    GitReference<GitCommitObject> ReferenceObject { get; }

    bool OriginRefsOnly { get; }

    bool PinToMergeHead { get; }
}
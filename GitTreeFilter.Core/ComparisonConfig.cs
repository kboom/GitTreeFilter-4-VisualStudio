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

    /// <summary>
    /// Whether to scan the entire working tree for changes that were not staged.
    /// For larger projects this can take significantly more time (Lib2GitSharp is slow there).
    /// 
    /// What is a better strategy is to stage the files we're modifying.
    /// </summary>
    bool IncludeUnstagedChanges { get; }
}
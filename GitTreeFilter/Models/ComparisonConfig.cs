using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using Microsoft;
using System;

namespace GitTreeFilter;

internal class ComparisonConfig : IComparisonConfig
{
    internal ComparisonConfig(
        Func<IGlobalSettings> globalSettings,
        Func<ISessionSettings> sessionSettings,
        Func<GitReference<GitCommitObject>> referenceObject)
    {
        Assumes.NotNull(globalSettings);
        Assumes.NotNull(sessionSettings);
        Assumes.NotNull(referenceObject);

        _globalSettings = globalSettings;
        _sessionSettings = sessionSettings;
        _referenceObject = referenceObject;
    }

    public GitReference<GitCommitObject> ReferenceObject => _referenceObject();

    public bool OriginRefsOnly => _globalSettings().OriginRefsOnly;

    public bool PinToMergeHead => _sessionSettings().PinToMergeHead;

    private readonly Func<IGlobalSettings> _globalSettings;
    private readonly Func<ISessionSettings> _sessionSettings;
    private readonly Func<GitReference<GitCommitObject>> _referenceObject;
}

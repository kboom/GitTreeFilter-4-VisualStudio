using System;

namespace GitTreeFilter.Commands;

internal class GitFiltersControls
{
    public const string GuidGitFiltersControlsCmdSet = "04ed3748-c3e2-4f02-8aaf-552c892418b8";

    public readonly static Guid GitFiltersControlsCmdSet = new Guid(GuidGitFiltersControlsCmdSet);

    public const uint SelectReferenceObjectButtonId = 0x1022;

    public const int CommandIdPhysicalFileDiffMenuCommand = 0x0100;
    public const int CommandIdProjectFileDiffMenuCommand = 0x0200;
    public const int CommandIdSelectReferenceObjectCommand = 0x1022;
}

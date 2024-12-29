namespace GitTreeFilter.Core.Models;

/// <summary>
/// Configuration settings for the current plugin usage.
/// These settings include selected references or preferences for handling them.
/// Note that these options are not available in the Visual Studio settings pages.
/// Instead, they can be configured within various parts of the plugin interface itself.
/// </summary>
///     public interface ISessionSettings
public interface ISessionSettings
{
    bool IncludeUnstagedChanges { get; }
}


public sealed class SessionSettings : ISessionSettings
{
    public static SessionSettings Default = new();

    private SessionSettings(ISessionSettings settings)
    {
        IncludeUnstagedChanges = settings.IncludeUnstagedChanges;
    }

    private SessionSettings()
    {

    }

    public bool IncludeUnstagedChanges { get; private set; }

    public static SessionSettings CreateFrom(ISessionSettings sessionSettings) => new(sessionSettings);

    public SessionSettings WithIncludeUnstagedChanges(bool value)
    {
        return new SessionSettings(this)
        {
            IncludeUnstagedChanges = value
        };
    }
}

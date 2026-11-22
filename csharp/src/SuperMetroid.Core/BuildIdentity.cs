namespace SuperMetroid.Core;

/// <summary>
/// The identity of this build of the game's simulation (#2602), written as the game is built: it changes exactly when the game's code does,
/// so a report, a recording or a content identity names the build that made it.
/// </summary>
public static partial class BuildIdentity
{
    /// <summary>The identity of the game's code as built.</summary>
    public static partial Guid Version { get; }
}

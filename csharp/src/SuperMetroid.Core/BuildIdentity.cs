namespace SuperMetroid.Core;

/// <summary>
/// The identity of this build of the game's simulation (#2602), which the compiler writes: it changes exactly when the built code does,
/// so a report, a recording or a content identity names the build that made it without reading the assembly.
/// </summary>
public static partial class BuildIdentity
{
    /// <summary>The identity of the compiled code.</summary>
    public static partial Guid Version { get; }
}

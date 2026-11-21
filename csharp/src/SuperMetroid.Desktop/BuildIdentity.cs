namespace SuperMetroid.Desktop;

/// <summary>
/// The identity of this build of the desktop host (#2602), which the compiler writes: it changes exactly when the built code does,
/// so a report, a recording or a content identity names the build that made it without reading the assembly.
/// </summary>
internal static partial class BuildIdentity
{
    /// <summary>The identity of the compiled code.</summary>
    internal static partial Guid Version { get; }
    /// <summary>The build's informational version, as the build stamps it on this assembly.</summary>
    internal static partial string InformationalVersion { get; }
}

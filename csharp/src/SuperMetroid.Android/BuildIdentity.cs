namespace SuperMetroid.Android;

/// <summary>
/// The identity of this build of the Android host (#2602), which the compiler writes: it changes exactly when the built code does,
/// so a recording names the build that made it without reading the assembly.
/// </summary>
internal static partial class BuildIdentity
{
    /// <summary>The identity of the compiled code.</summary>
    internal static partial Guid Version { get; }
}

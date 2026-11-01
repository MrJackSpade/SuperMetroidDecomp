namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled NTSC beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>
    /// The horizontal and vertical speed every retail beam combination launches at, in 256ths of a pixel per update:
    /// four pixels.
    /// </summary>
    internal const int CardinalSpeed = 1024;
    /// <summary>The diagonal speed every retail beam combination launches at, in 256ths of a pixel per update.</summary>
    internal const int DiagonalSpeed = 683;
    /// <summary>The 8.8 base speed a fired missile ignites at: one pixel per update.</summary>
    internal const int MissileIgnitionSpeed = 256;
    /// <summary>
    /// The 8.8 base speed a reflected missile re-ignites at: one pixel per update plus the 240/256 head start the
    /// reflection leaves.
    /// </summary>
    internal const int ReflectedMissileIgnitionSpeed = 496;
}

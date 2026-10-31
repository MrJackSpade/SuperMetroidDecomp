namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled NTSC beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>$90:C2D1 BeamSpeeds_Horizontal_Vertical: all twelve authored rows use 4 px/frame.</summary>
    internal const int CardinalSpeed = 1024;
    /// <summary>$90:C2D3 BeamSpeeds_Diagonal: all twelve authored rows use 683/256 px/frame.</summary>
    internal const int DiagonalSpeed = 683;
    /// <summary>$90:C301 MissileInitializedBitset: adjacent word reached by invalid beam combinations.</summary>
    internal const int MissileInitialized = 256;
    /// <summary>The 8.8 base speed a fired missile ignites at: one pixel per update.</summary>
    internal const int MissileIgnitionSpeed = 256;
    /// <summary>
    /// The 8.8 base speed a reflected missile re-ignites at: one pixel per update plus the 240/256 head start the
    /// reflection leaves.
    /// </summary>
    internal const int ReflectedMissileIgnitionSpeed = 496;
}

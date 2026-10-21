namespace SuperMetroid.Core.Samus;

/// <summary>
/// The sizes of the cartridge byte ranges Samus's glitch paths read as data. The bytes themselves are not part of the
/// port: the import pipeline reads them from the player's cartridge into <see cref="SamusOverreadCatalog"/>.
/// </summary>
internal static class SamusOverreadDefinitions
{
    /// <summary>
    /// Bytes the SpaceTime palette-copy overrun can read: the bank-$80 program from the first byte the overrun reads
    /// through the end of the bank, seen through its bank-$00 mirror.
    /// </summary>
    internal const int SpacetimeCopySourceByteCount = 28367;

    /// <summary>
    /// Colours the Speed Booster palette overrun reads from the grapple handler's code when Gravity Speed Booster
    /// inherits Screw Attack phase ten: one OBJ palette's worth.
    /// </summary>
    internal const int SpeedBoosterGrappleCodeColorCount = 16;

    /// <summary>Bytes the HUD's posture-transition admission reads before the twelve real flags, one per pose before them.</summary>
    internal const int HudPrecedingPostureByteCount = 53;

    /// <summary>Bytes the HUD's posture-transition admission reads after the twelve real flags, one per pose up to the non-firing transitions.</summary>
    internal const int HudFollowingPostureByteCount = 154;

    /// <summary>Bytes in one trail overrun entry: its kind, then the left and right sparkles' signed X and Y offsets.</summary>
    internal const int TrailOverrunEntryByteCount = 5;
}

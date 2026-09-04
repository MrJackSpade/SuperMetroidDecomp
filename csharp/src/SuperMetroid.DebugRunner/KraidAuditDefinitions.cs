/// <summary>
/// Retail room and door identities used by the Kraid end-to-end audit. These are kept
/// separate from the audit behavior so call sites state which cartridge object they use
/// instead of relying on raw addresses and adjacent explanatory comments.
/// </summary>
internal static class KraidAuditDefinitions
{
    /// <summary>Left edge of issue #268's formerly corrupt post-growth BG2 rectangle.</summary>
    public const int ArtifactRegionLeft = 48;

    /// <summary>Exclusive right edge of issue #268's formerly corrupt rectangle.</summary>
    public const int ArtifactRegionRight = 240;

    /// <summary>Top edge of the stable artifact-only band below Samus after growth.</summary>
    public const int ArtifactRegionTop = 96;

    /// <summary>Exclusive bottom edge of the stable artifact-only band above Kraid.</summary>
    public const int ArtifactRegionBottom = 136;

    /// <summary>Minimum opaque BG1 pixels required for the region assertion to be meaningful.</summary>
    public const int MinimumArtifactRegionBg1Pixels = 1000;

    /// <summary>Kraid-room door <c>$83:91CE</c>, which exits left after the fight.</summary>
    public const ushort LeftExitDoor = 0x91ce;

    /// <summary>Destination room <c>$8F:A56B</c> of Kraid's left exit.</summary>
    public const ushort LeftExitDestination = 0xa56b;

    /// <summary>Kraid-room door <c>$83:91DA</c>, which exits right after the fight.</summary>
    public const ushort RightExitDoor = 0x91da;

    /// <summary>Destination room <c>$8F:A6E2</c> of Kraid's right exit.</summary>
    public const ushort RightExitDestination = 0xa6e2;
}

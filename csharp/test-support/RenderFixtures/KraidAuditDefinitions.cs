/// <summary>
/// Retail room and door identities used by the Kraid end-to-end audit. These are kept
/// separate from the audit behavior so call sites state which cartridge object they use
/// instead of relying on raw addresses and adjacent explanatory comments.
/// </summary>
internal static class KraidAuditDefinitions
{
    /// <summary>Incoming door $83:91B6, entering Kraid's lower-left screen.</summary>
    public const ushort EntryDoor = 0x91b6;

    /// <summary>First gameplay scanline below the HUD in the entry-wrap reproduction.</summary>
    public const int EntryCeilingBandTop = 32;

    /// <summary>Exclusive bottom of the 16-pixel entry-wrap observation band.</summary>
    public const int EntryCeilingBandBottom = 48;

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

    /// <summary>First CGRAM color used by the HUD's selected-item palette four.</summary>
    public const int SelectedHudPaletteFirstColor = 17;

    /// <summary>First CGRAM color used by the HUD's unselected-item palette five.</summary>
    public const int UnselectedHudPaletteFirstColor = 21;

    /// <summary>Number of visible colors in either two-bit HUD item palette.</summary>
    public const int HudItemVisibleColorCount = 3;

    /// <summary>Screen-space left edge of the three-tile missile icon.</summary>
    public const int MissileIconLeft = 80;

    /// <summary>Screen-space left edge of the two-tile super-missile icon.</summary>
    public const int SuperMissileIconLeft = 112;

    /// <summary>Screen-space top edge shared by HUD item icons.</summary>
    public const int HudItemIconTop = 8;

    /// <summary>Pixel width of the missile icon.</summary>
    public const int MissileIconWidth = 24;

    /// <summary>Pixel width of an ordinary two-tile item icon.</summary>
    public const int StandardItemIconWidth = 16;

    /// <summary>Pixel height shared by HUD item icons.</summary>
    public const int HudItemIconHeight = 16;

    /// <summary>Kraid-room door <c>$83:91CE</c>, which exits left after the fight.</summary>
    public const ushort LeftExitDoor = 0x91ce;

    /// <summary>Destination room <c>$8F:A56B</c> of Kraid's left exit.</summary>
    public const ushort LeftExitDestination = 0xa56b;

    /// <summary>Kraid-room door <c>$83:91DA</c>, which exits right after the fight.</summary>
    public const ushort RightExitDoor = 0x91da;

    /// <summary>Destination room <c>$8F:A6E2</c> of Kraid's right exit.</summary>
    public const ushort RightExitDestination = 0xa6e2;
}

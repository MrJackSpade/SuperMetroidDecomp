using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where the post-credits text shows on the screen (#1416, #1467, #1555), in screen pixels before the text is moved down,
/// the items the item percentage counts, and the clear-time digit positions.
/// </summary>
public static class EndingResultTextLayout
{
    /// <summary>The side of one glyph of the ending's fixed-width text in pixels; a typed character's column and row count glyphs.</summary>
    public const int GlyphPixels = 8;

    /// <summary>The upper-left corner of the nine-row result panel: the left edge, nine glyphs down.</summary>
    public static Position16<ScreenPixels> ResultPanelCorner { get; } = new(0, 9 * GlyphPixels);

    /// <summary>The upper-left corner of the two-row copyright panel: the left edge, twelve glyphs down.</summary>
    public static Position16<ScreenPixels> CopyrightPanelCorner { get; } = new(0, 12 * GlyphPixels);

    /// <summary>The upper-left corner of the two-row Japanese subtitle: the left edge, twenty-three glyphs down.</summary>
    public static Position16<ScreenPixels> JapaneseSubtitleCorner { get; } = new(0, 23 * GlyphPixels);

    /// <summary>The upper-left corner of the item percentage's hundreds digit; tens, units and the percent sign follow to its right.</summary>
    public static Position16<ScreenPixels> PercentageHundredsCorner { get; } = new(14 * GlyphPixels, 14 * GlyphPixels);

    /// <summary>The upper-left corner of the typed glyph <paramref name="column"/> glyphs across and <paramref name="row"/> down.</summary>
    public static Position16<ScreenPixels> GlyphCorner(int column, int row) => new(column * GlyphPixels, row * GlyphPixels);

    /// <summary>The items that each count one toward the ending item percentage; ammo and energy pickups are counted separately.</summary>
    public const SamusEquipmentFlags CollectibleItemMask =
        SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall |
        SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster |
        SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope;

    /// <summary>The beams that each count one toward the ending item percentage.</summary>
    public const SamusBeamFlags CollectibleBeamMask =
        SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Charge;

    /// <summary>The clear time's hours tens digit's screen X.</summary>
    public const int HoursTensX = 156;

    /// <summary>The clear time's hours units digit's screen X.</summary>
    public const int HoursUnitsX = 164;

    /// <summary>The clear time's minutes tens digit's screen X, to the right of the colon.</summary>
    public const int MinutesTensX = 180;

    /// <summary>The clear time's minutes units digit's screen X.</summary>
    public const int MinutesUnitsX = 188;
}

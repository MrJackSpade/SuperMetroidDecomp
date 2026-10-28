namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The selected material channels of Norfair Ridley's room reveal; <see cref="NorfairRidleyRevealPaint"/> calculates the reveal rows from them.</summary>
internal static class NorfairRidleyRevealPaintDefinitions
{
    /// <summary>Fifteen fade rows precede the full arena reveal palette.</summary>
    internal const int Rows = 15;
    /// <summary>Each reveal row copies fourteen room-theme colors.</summary>
    internal const int Colors = 14;
    /// <summary>Maximum value of an SNES five-bit color channel.</summary>
    internal const int Maximum = (1 << 5) - 1;
    /// <summary>
    /// $A6:A693-A698: warm trim's selected red22/blue1 and green12/6/2 shades.
    /// Slots2/3 mark small trim highlights in $B9:BBA5; slot1 is the same
    /// material's copied bright target, without a visible-use claim for that map.
    /// </summary>
    internal const int TrimRed = 22, TrimBlue = 1,
        TrimHighlightGreen = 12, TrimMiddleGreen = 6, TrimShadowGreen = 2;
    /// <summary>
    /// $A6:A699-A6A0: three masonry/pillar shades and black, with red step4
    /// and blue step2 above the shared blue1 dark tint. These selected paint
    /// steps describe the native dithered material, not physical illumination.
    /// </summary>
    internal const int MasonryRedStep = 4, MasonryBlueStep = 2;
    /// <summary>
    /// $A6:A6A1-A6A8: cavern-edge highlight red14/blue2, red shade step5 and
    /// one-unit blue decline, clipped to black. Replacing the selected material
    /// levels or grouping would invent different categorical artwork content.
    /// </summary>
    internal const int CavernHighlightRed = 14, CavernRedStep = 5, CavernHighlightBlue = 2;
}

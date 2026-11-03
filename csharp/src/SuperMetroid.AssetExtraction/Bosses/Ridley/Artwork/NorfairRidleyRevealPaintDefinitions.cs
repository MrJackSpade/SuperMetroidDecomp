namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>The selected material channels of Norfair Ridley's room reveal; <see cref="NorfairRidleyRevealPaint"/> calculates the reveal steps from them.</summary>
internal static class NorfairRidleyRevealPaintDefinitions
{
    /// <summary>Fifteen fade steps precede the full arena reveal palette.</summary>
    internal const int Steps = 15;
    /// <summary>Each reveal step copies fourteen room-theme colors.</summary>
    internal const int Colors = 14;
    /// <summary>Maximum value of a five-bit color channel.</summary>
    internal const int Maximum = (1 << 5) - 1;
    /// <summary>
    /// Warm trim's selected red22/blue1 and green12/6/2 shades.
    /// Slots2/3 mark small trim highlights in the room's tiles; slot1 is the same
    /// material's copied bright target, without a visible-use claim for that map.
    /// </summary>
    internal const int TrimRed = 22, TrimBlue = 1,
        TrimHighlightGreen = 12, TrimMiddleGreen = 6, TrimShadowGreen = 2;
    /// <summary>
    /// Three masonry/pillar shades and black, with red step4
    /// and blue step2 above the shared blue1 dark tint. These selected paint
    /// steps describe the dithered material, not physical illumination.
    /// </summary>
    internal const int MasonryRedStep = 4, MasonryBlueStep = 2;
    /// <summary>
    /// Cavern-edge highlight red14/blue2, red shade step5 and
    /// one-unit blue decline, clipped to black. Replacing the selected material
    /// levels or grouping would invent different categorical artwork content.
    /// </summary>
    internal const int CavernHighlightRed = 14, CavernRedStep = 5, CavernHighlightBlue = 2;
}

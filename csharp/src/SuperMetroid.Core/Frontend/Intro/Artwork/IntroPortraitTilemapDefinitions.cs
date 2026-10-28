namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>$97:88CC Samus-head BG2 composition, copied by $8B:A4A5-A4B3 to VRAM word $4800. Atlas runs and repeated cells calculate; selected drawing layout remains explicit.</summary>
internal static class IntroPortraitTilemapDefinitions
{
    /// <summary>$97:88CC: selected portrait atlas block begins at $300; its sixteen-cell row stride comes from the source atlas.</summary>
    internal const int AtlasOrigin = 0x300;
    /// <summary>$97:88CC: the main sixteen-column portrait block starts at page column eight, centered in the 32-column BG page.</summary>
    internal const int AtlasSide = 16;
    /// <summary>$97:88CC: chosen portrait top row five.</summary>
    internal const int Top = 5;
    /// <summary>$97:88CC: chosen blank tile $3FE; RGB paint and tile pixels are separate.</summary>
    internal const int BlankTile = 0x3fe;
    /// <summary>The portrait's colour variant three, in the lower priority tier and unmirrored.</summary>
    internal const int Palette = 3;

    /// <summary>$97:88CC: selected one-bit occupancy of the main sixteen-by-sixteen atlas block. Repeated adjacent rows share their silhouette; every occupied tile identity follows the same atlas position formula. This mask describes drawing placement, not collision or a sampled numeric curve.</summary>
    /// <remarks>Bit <c>c</c> of row <c>r</c> marks atlas column <c>c</c> of the row, so each row reads right to left.</remarks>
    internal static readonly int[] SilhouetteRows =
    [
        0b0000_0111_1111_0000,
        0b0000_0011_1111_1000,
        0b0000_0011_1111_1100,
        0b0000_0011_1111_1100,
        0b0000_0011_1111_1110,
        0b1100_0001_1111_1110,
        0b1111_0000_1111_1111,
        0b1111_1100_1111_1111,
        0b1001_1111_1111_1111,
        0b1101_1111_1111_1111,
        0b1101_1111_1111_1111,
        0b0110_1111_1111_1111,
        0b0110_0111_1111_1111,
        0b0111_0011_1111_1111,
        0b0011_1011_1111_1111,
        0b0001_1111_1111_1000,
    ];
}

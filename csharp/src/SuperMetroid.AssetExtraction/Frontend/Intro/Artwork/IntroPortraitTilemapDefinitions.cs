namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>$97:88CC Samus-head BG2 composition, copied by $8B:A4A5-A4B3 to VRAM word $4800. Atlas runs and repeated cells calculate; selected drawing layout remains explicit.</summary>
internal static class IntroPortraitTilemapDefinitions
{
    /// <summary>$97:88CC: selected portrait atlas block begins at $300; its sixteen-cell row stride comes from the source atlas.</summary>
    internal const int AtlasOrigin = 768;
    /// <summary>$97:88CC: the main sixteen-column portrait block starts at page column eight, centered in the 32-column BG page.</summary>
    internal const int AtlasSide = 16;
    /// <summary>$97:88CC: chosen portrait top row five.</summary>
    internal const int Top = 5;
    /// <summary>$97:88CC: chosen blank tile $3FE; RGB paint and tile pixels are separate.</summary>
    internal const int BlankTile = 1022;
    /// <summary>The portrait's colour variant three, in the lower priority tier and unmirrored.</summary>
    internal const int Palette = 3;

    /// <summary>$97:88CC: selected one-bit occupancy of the main sixteen-by-sixteen atlas block. Repeated adjacent rows share their silhouette; every occupied tile identity follows the same atlas position formula. This mask describes drawing placement, not collision or a sampled numeric curve.</summary>
    /// <remarks>Character <c>c</c> of row <c>r</c> is atlas column <c>c</c> of the row, left to right; <see cref="OccupiedCell"/> marks an occupied cell.</remarks>
    internal static readonly string[] SilhouetteRows =
    [
        "....#######.....",
        "...#######......",
        "..########......",
        "..########......",
        ".#########......",
        ".########.....##",
        "########....####",
        "########..######",
        "#############..#",
        "#############.##",
        "#############.##",
        "############.##.",
        "###########..##.",
        "##########..###.",
        "##########.###..",
        "...##########...",
    ];

    /// <summary>The silhouette character marking an occupied atlas cell.</summary>
    internal const char OccupiedCell = '#';
}

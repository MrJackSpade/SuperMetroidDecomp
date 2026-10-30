using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The tiles the cartridge draws the SPACE COLONY caption with (#1467): the import pictures each caption letter and the
/// katakana caption from them. Glyph identities come from the one-cell drawing operations 8C:D7C1..D7F6; every tile
/// selects the high-priority plane in colour variant 0.
/// </summary>
internal static class SpaceColonyCaptionTileDefinitions
{
    /// <summary>$8C:D7FB: the Japanese caption's two rows of tiles, each a high-priority character in colour variant 0.</summary>
    internal static ReadOnlySpan<BackgroundTile> JapaneseTiles => japaneseTiles;

    /// <summary>The tiles behind <see cref="JapaneseTiles"/>.</summary>
    private static readonly BackgroundTile[] japaneseTiles =
    [
        Caption(27, 10), Caption(27, 11), Caption(27, 3), Caption(27, 10), Caption(27, 12), Caption(14, 0), Caption(14, 1), Caption(27, 3),
        Caption(14, 2), Caption(14, 3), Caption(27, 4), Caption(14, 2), Caption(14, 4), Caption(26, 3), Caption(26, 4), Caption(27, 4),
    ];

    /// <summary>The high-priority tile, in colour variant 0, of the caption character at <paramref name="row"/> and <paramref name="column"/>.</summary>
    private static BackgroundTile Caption(int row, int column) => new(CharacterSheetGrid.Cell(row, column), 0, highPriority: true);

    /// <summary>Native SPACE COLONY glyphs, each a high-priority tile in colour variant 0.
    /// Repeated C and O characters select the same drawing operation.</summary>
    internal static readonly IReadOnlyDictionary<char, BackgroundTile> Glyphs = new Dictionary<char, BackgroundTile>
    {
        ['S'] = new(0x1ed, 0, highPriority: true),
        ['P'] = new(0x1ee, 0, highPriority: true),
        ['A'] = new(0x1ef, 0, highPriority: true),
        ['C'] = new(0x1f7, 0, highPriority: true),
        ['E'] = new(0x1f8, 0, highPriority: true),
        ['O'] = new(0x1f9, 0, highPriority: true),
        ['L'] = new(0x1fa, 0, highPriority: true),
        ['N'] = new(0x1fb, 0, highPriority: true),
        ['Y'] = new(0x1b9, 0, highPriority: true),
    };
}

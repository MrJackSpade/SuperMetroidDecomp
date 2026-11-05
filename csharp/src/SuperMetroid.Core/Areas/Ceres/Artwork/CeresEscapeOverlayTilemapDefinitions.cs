using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The identity and placement of the English and Japanese Ceres overlays (#1466): each page's name, which names its
/// picture, and the foreground cell it is written at (#1514). The wording and glyph tiles that spell the pages are the
/// import's.
/// </summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>English EMERGENCY title, written at the foreground's row six, column eleven.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", new Position16<TilemapCells>(11, 6));

    /// <summary>First Japanese self-destruct subtitle row's upper half, written at the foreground's row twenty, column ten.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", new Position16<TilemapCells>(10, 20));

    /// <summary>First subtitle row's lower half, the row below.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", JapaneseFirst.Cell + NextRow);

    /// <summary>Second subtitle row's upper half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", JapaneseSecond.Cell + NextRow);

    /// <summary>Second subtitle row's lower half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", JapaneseThird.Cell + NextRow);

    /// <summary>From one row of cells to the next.</summary>
    private static Offset16<TilemapCells> NextRow => new(0, 1);

    /// <summary>The Japanese subtitle's four row halves, in the order the one-shot list writes them.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> Japanese { get; } =
        [JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
}

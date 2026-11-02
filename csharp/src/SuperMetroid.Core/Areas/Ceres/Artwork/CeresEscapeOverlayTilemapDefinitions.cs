using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The identity and placement of the English and Japanese Ceres overlays (#1466): each page's name, which names its
/// picture, and the BG1 cell it is written at. The wording and glyph tiles that spell the pages are the import's.
/// </summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>English EMERGENCY title tilemap, written at BG1 row six, column eleven.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", 6 * RowCells + 11);

    /// <summary>First Japanese self-destruct subtitle row's upper half, written at BG1 row twenty, column ten.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", 20 * RowCells + 10);

    /// <summary>First subtitle row's lower half, the row below.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", JapaneseFirst.Cell + RowCells);

    /// <summary>Second subtitle row's upper half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", JapaneseSecond.Cell + RowCells);

    /// <summary>Second subtitle row's lower half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", JapaneseThird.Cell + RowCells);

    /// <summary>The cells in one overlay row.</summary>
    private const int RowCells = 32;
    /// <summary>The Japanese subtitle's four row halves, in the order the one-shot list writes them.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> Japanese { get; } =
        [JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
}

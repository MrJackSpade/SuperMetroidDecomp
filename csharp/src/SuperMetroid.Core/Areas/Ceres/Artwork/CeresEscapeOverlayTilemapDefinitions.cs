using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The identity and placement of the English and Japanese Ceres overlays (#1466): each page's name, which names its
/// picture, its length and the BG1 cell it is written at. The glyph tiles that spell the pages are the import's.
/// </summary>
internal static class CeresEscapeOverlayTilemapDefinitions
{
    /// <summary>English EMERGENCY title tilemap, written at BG1 row six, column eleven.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition Emergency =
        new("emergency", EmergencyText.Length, 6 * RowCells + 11);

    /// <summary>First Japanese self-destruct subtitle row's upper half, written at BG1 row twenty, column ten.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFirst =
        new("japanese_0", JapaneseFirstLine.Length, 20 * RowCells + 10);

    /// <summary>First subtitle row's lower half, the row below.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseSecond =
        new("japanese_1", JapaneseFirstLine.Length, JapaneseFirst.Cell + RowCells);

    /// <summary>Second subtitle row's upper half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseThird =
        new("japanese_2", JapaneseSecondLine.Length, JapaneseSecond.Cell + RowCells);

    /// <summary>Second subtitle row's lower half, the row below that.</summary>
    internal static readonly CeresEscapeOverlayTilemapDefinition JapaneseFourth =
        new("japanese_3", JapaneseSecondLine.Length, JapaneseThird.Cell + RowCells);

    /// <summary>English warning title rendered from the Latin tile alphabet.</summary>
    /// <remarks>Authored warning wording and font ordering are categorical typography content. The selected two-half
    /// packing, foreground priority and Japanese palette role preserve that composition; glyph pixels and RGB paint
    /// remain separate obligations.</remarks>
    internal const string EmergencyText = "EMERGENCY";
    /// <summary>First authored Japanese self-destruct subtitle row, shared by its upper and lower tile halves.</summary>
    internal const string JapaneseFirstLine = "自爆装置が、作動しました";
    /// <summary>Second authored Japanese evacuation subtitle row, shared by its upper and lower tile halves.</summary>
    internal const string JapaneseSecondLine = "ただちに脱出して下さい";
    /// <summary>The cells in one overlay row.</summary>
    private const int RowCells = 32;
    /// <summary>The five fixed overlays in transfer order: the English title, then the four Japanese row halves.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> All { get; } =
        [Emergency, JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
    /// <summary>The Japanese subtitle's four row halves, in the order the one-shot list at $A6:C3B8 writes them.</summary>
    internal static ImmutableArray<CeresEscapeOverlayTilemapDefinition> Japanese { get; } =
        [JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
}

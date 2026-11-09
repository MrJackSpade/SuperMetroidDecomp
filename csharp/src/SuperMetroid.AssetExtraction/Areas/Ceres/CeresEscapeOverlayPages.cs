using System.Collections.Immutable;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using static SuperMetroid.Core.Areas.Ceres.Artwork.CeresEscapeWarningPictures;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The import's knowledge of the Ceres warning overlays (#1502): the authored wording each page spells, so its length,
/// and the five pages in the order the import exports them. The game only writes each page's picture at its room pixel.
/// </summary>
internal static class CeresEscapeOverlayPages
{
    /// <summary>English warning title rendered from the Latin tile alphabet.</summary>
    /// <remarks>Authored warning wording and font ordering are categorical typography content. The selected two-half
    /// packing, foreground priority and Japanese palette role preserve that composition; glyph pixels and RGB paint
    /// remain separate obligations.</remarks>
    internal const string EmergencyText = "EMERGENCY";

    /// <summary>First authored Japanese self-destruct subtitle row, shared by its upper and lower tile halves.</summary>
    internal const string JapaneseFirstLine = "自爆装置が、作動しました";

    /// <summary>Second authored Japanese evacuation subtitle row, shared by its upper and lower tile halves.</summary>
    internal const string JapaneseSecondLine = "ただちに脱出して下さい";

    /// <summary>The five fixed overlays in transfer order: the English title, then the four Japanese row halves.</summary>
    internal static ImmutableArray<CeresEscapeWarningPicture> All { get; } =
        [Emergency, JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];

    /// <summary>The authored wording one page spells: the title, or the subtitle row its two halves share.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not one of the overlays.</exception>
    internal static string Text(CeresEscapeWarningPicture page) =>
        page == Emergency ? EmergencyText
        : page == JapaneseFirst || page == JapaneseSecond ? JapaneseFirstLine
        : page == JapaneseThird || page == JapaneseFourth ? JapaneseSecondLine
        : throw new ArgumentOutOfRangeException(nameof(page), $"{page.Name} is not a Ceres escape overlay.");

    /// <summary>The words of a Ceres warning page, one per character of its wording.</summary>
    extension(CeresEscapeWarningPicture page)
    {
        /// <summary>The page's words.</summary>
        internal int WordCount => Text(page).Length;
    }
}

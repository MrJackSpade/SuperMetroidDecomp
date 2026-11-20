using System.Collections.Immutable;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using static SuperMetroid.Core.Areas.Ceres.Artwork.CeresEscapeWarningPictures;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The import's knowledge of the Ceres warning overlays (#1502, #2425): the authored wording each page spells, so its
/// width, the rows of words it spans, and the three pages in the order the import exports them. The game only writes each
/// page's picture at its room pixel.
/// </summary>
internal static class CeresEscapeOverlayPages
{
    /// <summary>English warning title rendered from the Latin tile alphabet.</summary>
    /// <remarks>Authored warning wording and font ordering are categorical typography content. The selected two-half
    /// packing, foreground priority and Japanese palette role preserve that composition; glyph pixels and RGB paint
    /// remain separate obligations.</remarks>
    internal const string EmergencyText = "EMERGENCY";

    /// <summary>The authored Japanese subtitle line saying the self-destruct has started.</summary>
    internal const string JapaneseSelfDestructText = "自爆装置が、作動しました";

    /// <summary>The authored Japanese subtitle line telling Samus to escape at once.</summary>
    internal const string JapaneseEscapeText = "ただちに脱出して下さい";

    /// <summary>The three fixed overlays in transfer order: the English title, then the Japanese subtitle's two lines.</summary>
    internal static ImmutableArray<CeresEscapeWarningPicture> All { get; } =
        [Emergency, JapaneseSelfDestructLine, JapaneseEscapeLine];

    /// <summary>The eight-pixel rows the English title's glyphs stand.</summary>
    private const int TitleGlyphRows = 1;

    /// <summary>The eight-pixel rows a Japanese subtitle line's sixteen-pixel glyphs stand: an upper and a lower row.</summary>
    private const int SubtitleGlyphRows = 2;

    /// <summary>
    /// Each overlay's wording and the eight-pixel rows its glyphs stand: one for the title, two for a subtitle line, whose
    /// sixteen-pixel glyphs are an upper and a lower row.
    /// </summary>
    private static readonly IReadOnlyDictionary<CeresEscapeWarningPicture, (string Text, int GlyphRows)> Wordings =
        new Dictionary<CeresEscapeWarningPicture, (string Text, int GlyphRows)>
        {
            [Emergency] = (EmergencyText, TitleGlyphRows),
            [JapaneseSelfDestructLine] = (JapaneseSelfDestructText, SubtitleGlyphRows),
            [JapaneseEscapeLine] = (JapaneseEscapeText, SubtitleGlyphRows),
        };

    /// <summary>The authored wording one page spells: the title, or a subtitle line.</summary>
    /// <exception cref="KeyNotFoundException"><paramref name="page"/> is not one of the overlays.</exception>
    internal static string Text(CeresEscapeWarningPicture page) => Wordings[page].Text;

    /// <summary>The words of a Ceres warning page: rows of words, each one word per character of its wording.</summary>
    extension(CeresEscapeWarningPicture page)
    {
        /// <summary>The words across each of the page's rows, one per character of its wording.</summary>
        internal int Width => Text(page).Length;

        /// <summary>
        /// The eight-pixel rows of words the page spans: one for the title, two for a subtitle line, whose sixteen-pixel
        /// glyphs are an upper and a lower row.
        /// </summary>
        internal int RowCount => Wordings[page].GlyphRows;

        /// <summary>The page's words, row by row from the top.</summary>
        internal int WordCount => page.Width * page.RowCount;
    }
}

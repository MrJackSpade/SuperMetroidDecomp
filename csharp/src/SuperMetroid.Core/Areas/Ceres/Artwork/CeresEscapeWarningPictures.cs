using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The English and Japanese Ceres escape warning's pictures (#1466, #1555, #2425): each picture's name and the pixel of the
/// room's foreground its top-left corner shows at (#1514, #1601). The wording and glyphs that spell them are the import's.
/// </summary>
internal static class CeresEscapeWarningPictures
{
    /// <summary>English EMERGENCY title, 88 pixels from the foreground's left and 48 down.</summary>
    internal static readonly CeresEscapeWarningPicture Emergency =
        new("emergency", new Position16<PlanePixels>(88, 48));

    /// <summary>
    /// The Japanese subtitle's first line, saying the self-destruct has started, 80 pixels from the foreground's left and
    /// 160 down.
    /// </summary>
    internal static readonly CeresEscapeWarningPicture JapaneseSelfDestructLine =
        new("japanese_self_destruct", new Position16<PlanePixels>(80, 160));

    /// <summary>The subtitle's second line, telling Samus to escape at once, one line of glyphs below the first.</summary>
    internal static readonly CeresEscapeWarningPicture JapaneseEscapeLine =
        new("japanese_escape", JapaneseSelfDestructLine.Corner + NextLine);

    /// <summary>From one line of the subtitle's sixteen-pixel glyphs to the next.</summary>
    private static Offset16<PlanePixels> NextLine => new(0, 16);

    /// <summary>The Japanese subtitle's two lines, the first on top, which the getaway shows together.</summary>
    internal static ImmutableArray<CeresEscapeWarningPicture> JapaneseSubtitle { get; } =
        [JapaneseSelfDestructLine, JapaneseEscapeLine];
}

using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The English and Japanese Ceres escape warning's pictures (#1466, #1555): each picture's name and the pixel of the
/// room's foreground its top-left corner shows at (#1514, #1601). The wording and glyphs that spell them are the import's.
/// </summary>
internal static class CeresEscapeWarningPictures
{
    /// <summary>English EMERGENCY title, 88 pixels from the foreground's left and 48 down.</summary>
    internal static readonly CeresEscapeWarningPicture Emergency =
        new("emergency", new Position16<PlanePixels>(88, 48));

    /// <summary>First Japanese self-destruct subtitle row's upper half, 80 pixels from the foreground's left and 160 down.</summary>
    internal static readonly CeresEscapeWarningPicture JapaneseFirst =
        new("japanese_0", new Position16<PlanePixels>(80, 160));

    /// <summary>First subtitle row's lower half, one glyph row below.</summary>
    internal static readonly CeresEscapeWarningPicture JapaneseSecond =
        new("japanese_1", JapaneseFirst.Corner + NextRow);

    /// <summary>Second subtitle row's upper half, one glyph row below that.</summary>
    internal static readonly CeresEscapeWarningPicture JapaneseThird =
        new("japanese_2", JapaneseSecond.Corner + NextRow);

    /// <summary>Second subtitle row's lower half, one glyph row below that.</summary>
    internal static readonly CeresEscapeWarningPicture JapaneseFourth =
        new("japanese_3", JapaneseThird.Corner + NextRow);

    /// <summary>From one eight-pixel row of glyphs to the next.</summary>
    private static Offset16<PlanePixels> NextRow => new(0, 8);

    /// <summary>The Japanese subtitle's four row halves, in the order the getaway writes them.</summary>
    internal static ImmutableArray<CeresEscapeWarningPicture> Japanese { get; } =
        [JapaneseFirst, JapaneseSecond, JapaneseThird, JapaneseFourth];
}

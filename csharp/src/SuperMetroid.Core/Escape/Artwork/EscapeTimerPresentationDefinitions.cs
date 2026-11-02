namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Stable sheet frame names and stock anchors for the escape timer presentation.</summary>
public static class EscapeTimerPresentationDefinitions
{
    /// <summary>The sheet frame name of the TIME label and both separators.</summary>
    public const string LabelFrame = "Label";

    /// <summary>The digit glyphs, indexed by the decimal value each draws.</summary>
    internal static readonly EscapeTimerGlyph[] DigitGlyphs =
    [
        EscapeTimerGlyph.Digit0, EscapeTimerGlyph.Digit1, EscapeTimerGlyph.Digit2, EscapeTimerGlyph.Digit3, EscapeTimerGlyph.Digit4,
        EscapeTimerGlyph.Digit5, EscapeTimerGlyph.Digit6, EscapeTimerGlyph.Digit7, EscapeTimerGlyph.Digit8, EscapeTimerGlyph.Digit9,
    ];

    /// <summary>The sheet frame name of each glyph: Label, or Digit.0 through Digit.9.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerGlyph, string> FrameNames = new Dictionary<EscapeTimerGlyph, string>
    {
        [EscapeTimerGlyph.Label] = LabelFrame,
        [EscapeTimerGlyph.Digit0] = "Digit.0",
        [EscapeTimerGlyph.Digit1] = "Digit.1",
        [EscapeTimerGlyph.Digit2] = "Digit.2",
        [EscapeTimerGlyph.Digit3] = "Digit.3",
        [EscapeTimerGlyph.Digit4] = "Digit.4",
        [EscapeTimerGlyph.Digit5] = "Digit.5",
        [EscapeTimerGlyph.Digit6] = "Digit.6",
        [EscapeTimerGlyph.Digit7] = "Digit.7",
        [EscapeTimerGlyph.Digit8] = "Digit.8",
        [EscapeTimerGlyph.Digit9] = "Digit.9",
    };

    /// <summary>Decimal glyphs are eight pixels wide.</summary>
    private const int GlyphWidth = 8;

    /// <summary>Three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerAnchor, Offset16<SpritePixels>> DefaultAnchors =
        new Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>>
        {
            [EscapeTimerAnchor.Label] = new(0, 0),
            [EscapeTimerAnchor.Minutes] = GroupAnchor(0),
            [EscapeTimerAnchor.Seconds] = GroupAnchor(1),
            [EscapeTimerAnchor.Centiseconds] = GroupAnchor(2),
        };

    /// <summary>The tens-digit origin of the <paramref name="group"/>th two-digit group.</summary>
    private static Offset16<SpritePixels> GroupAnchor(int group) => new((3 * group * 2 - 7) * GlyphWidth / 2, 0);
}

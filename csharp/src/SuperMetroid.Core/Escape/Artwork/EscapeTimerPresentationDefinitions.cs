namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Stable names, extraction sources and stock anchors for the escape timer presentation.</summary>
public static class EscapeTimerPresentationDefinitions
{
    /// <summary>Supported JSON revision for the timer's pixel anchors and digit spacing; its frames are the installed sheet.</summary>
    public const int Version = 2;
    /// <summary>Installation-relative JSON layout filename, separate from the timer font's indexed PNG artwork.</summary>
    public const string FileName = "escape-timer.json";

    /// <summary>The sheet frame name of the TIME label and both separators.</summary>
    public const string LabelFrame = "Label";
    /// <summary>Every composition: the TIME label, then the digits 0 through 9.</summary>
    public static IReadOnlyList<EscapeTimerGlyph> Glyphs { get; } = Enum.GetValues<EscapeTimerGlyph>();
    /// <summary>Every anchor: the label origin, then the tens-digit origins of the three time pairs.</summary>
    public static IReadOnlyList<EscapeTimerAnchor> Anchors { get; } = Enum.GetValues<EscapeTimerAnchor>();

    /// <summary>The digit glyphs, indexed by the decimal value each draws.</summary>
    internal static readonly EscapeTimerGlyph[] DigitGlyphs =
    [
        EscapeTimerGlyph.Digit0, EscapeTimerGlyph.Digit1, EscapeTimerGlyph.Digit2, EscapeTimerGlyph.Digit3, EscapeTimerGlyph.Digit4,
        EscapeTimerGlyph.Digit5, EscapeTimerGlyph.Digit6, EscapeTimerGlyph.Digit7, EscapeTimerGlyph.Digit8, EscapeTimerGlyph.Digit9,
    ];

    /// <summary>The decimal value each digit glyph draws; the label draws none.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerGlyph, int> DigitValues = new Dictionary<EscapeTimerGlyph, int>
    {
        [EscapeTimerGlyph.Digit0] = 0,
        [EscapeTimerGlyph.Digit1] = 1,
        [EscapeTimerGlyph.Digit2] = 2,
        [EscapeTimerGlyph.Digit3] = 3,
        [EscapeTimerGlyph.Digit4] = 4,
        [EscapeTimerGlyph.Digit5] = 5,
        [EscapeTimerGlyph.Digit6] = 6,
        [EscapeTimerGlyph.Digit7] = 7,
        [EscapeTimerGlyph.Digit8] = 8,
        [EscapeTimerGlyph.Digit9] = 9,
    };

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

    /// <summary>The JSON key of each anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerAnchor, string> AnchorNames = new Dictionary<EscapeTimerAnchor, string>
    {
        [EscapeTimerAnchor.Label] = "Label",
        [EscapeTimerAnchor.Minutes] = "Minutes",
        [EscapeTimerAnchor.Seconds] = "Seconds",
        [EscapeTimerAnchor.Centiseconds] = "Centiseconds",
    };

    /// <summary>$80:9FEA..A05F: decimal glyphs are eight pixels wide.</summary>
    private const int GlyphWidth = 8;

    /// <summary>$80:9F7B/9F84/9F8D: three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerAnchor, Offset16<SpritePixels>> DefaultAnchors =
        new Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>>
        {
            [EscapeTimerAnchor.Label] = new(0, 0),
            [EscapeTimerAnchor.Minutes] = GroupAnchor(0),
            [EscapeTimerAnchor.Seconds] = GroupAnchor(1),
            [EscapeTimerAnchor.Centiseconds] = GroupAnchor(2),
        };

    /// <summary>The tens-digit origin of the <paramref name="group"/>th two-digit group.</summary>
    private static Offset16<SpritePixels> GroupAnchor(int group) => new(checked((short)((3 * group * 2 - 7) * GlyphWidth / 2)), 0);
}

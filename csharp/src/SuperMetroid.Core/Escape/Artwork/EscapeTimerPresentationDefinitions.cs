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

    /// <summary>The glyph that draws the decimal value <paramref name="digit"/>.</summary>
    /// <param name="digit">Decoded decimal value 0 through 9, not a packed-BCD byte.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/> is outside 0..9.</exception>
    public static EscapeTimerGlyph DigitGlyph(int digit) =>
        (uint)digit < DigitGlyphs.Length ? DigitGlyphs[digit] : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>The digit glyphs, indexed by the decimal value each draws.</summary>
    private static readonly EscapeTimerGlyph[] DigitGlyphs =
    [
        EscapeTimerGlyph.Digit0, EscapeTimerGlyph.Digit1, EscapeTimerGlyph.Digit2, EscapeTimerGlyph.Digit3, EscapeTimerGlyph.Digit4,
        EscapeTimerGlyph.Digit5, EscapeTimerGlyph.Digit6, EscapeTimerGlyph.Digit7, EscapeTimerGlyph.Digit8, EscapeTimerGlyph.Digit9,
    ];

    /// <summary>The decimal value a digit glyph draws.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="glyph"/> is the label.</exception>
    public static int DigitValue(EscapeTimerGlyph glyph) => glyph switch
    {
        EscapeTimerGlyph.Digit0 => 0,
        EscapeTimerGlyph.Digit1 => 1,
        EscapeTimerGlyph.Digit2 => 2,
        EscapeTimerGlyph.Digit3 => 3,
        EscapeTimerGlyph.Digit4 => 4,
        EscapeTimerGlyph.Digit5 => 5,
        EscapeTimerGlyph.Digit6 => 6,
        EscapeTimerGlyph.Digit7 => 7,
        EscapeTimerGlyph.Digit8 => 8,
        EscapeTimerGlyph.Digit9 => 9,
        EscapeTimerGlyph.Label => throw new ArgumentOutOfRangeException(nameof(glyph)),
        _ => throw new ArgumentOutOfRangeException(nameof(glyph)),
    };

    /// <summary>The sheet frame name of a glyph: Label, or Digit.0 through Digit.9.</summary>
    public static string FrameName(EscapeTimerGlyph glyph) => glyph switch
    {
        EscapeTimerGlyph.Label => LabelFrame,
        EscapeTimerGlyph.Digit0 => "Digit.0",
        EscapeTimerGlyph.Digit1 => "Digit.1",
        EscapeTimerGlyph.Digit2 => "Digit.2",
        EscapeTimerGlyph.Digit3 => "Digit.3",
        EscapeTimerGlyph.Digit4 => "Digit.4",
        EscapeTimerGlyph.Digit5 => "Digit.5",
        EscapeTimerGlyph.Digit6 => "Digit.6",
        EscapeTimerGlyph.Digit7 => "Digit.7",
        EscapeTimerGlyph.Digit8 => "Digit.8",
        EscapeTimerGlyph.Digit9 => "Digit.9",
        _ => throw new ArgumentOutOfRangeException(nameof(glyph)),
    };

    /// <summary>The JSON key of an anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    public static string AnchorName(EscapeTimerAnchor anchor) => anchor switch
    {
        EscapeTimerAnchor.Label => "Label",
        EscapeTimerAnchor.Minutes => "Minutes",
        EscapeTimerAnchor.Seconds => "Seconds",
        EscapeTimerAnchor.Centiseconds => "Centiseconds",
        _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
    };
    /// <summary>$80:9FEA..A05F: decimal glyphs are eight pixels wide.</summary>
    private const int GlyphWidth = 8;

    /// <summary>$80:9F7B/9F84/9F8D: three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static Offset16<SpritePixels> DefaultAnchor(EscapeTimerAnchor anchor) => anchor switch
    {
        EscapeTimerAnchor.Label => new(0, 0),
        EscapeTimerAnchor.Minutes => GroupAnchor(0),
        EscapeTimerAnchor.Seconds => GroupAnchor(1),
        EscapeTimerAnchor.Centiseconds => GroupAnchor(2),
        _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
    };

    /// <summary>The tens-digit origin of the <paramref name="group"/>th two-digit group.</summary>
    private static Offset16<SpritePixels> GroupAnchor(int group) => new(checked((short)((3 * group * 2 - 7) * GlyphWidth / 2)), 0);
}

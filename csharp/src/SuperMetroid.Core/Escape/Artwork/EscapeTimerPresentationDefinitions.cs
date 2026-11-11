namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Stable sheet frame names for the escape timer presentation.</summary>
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
}

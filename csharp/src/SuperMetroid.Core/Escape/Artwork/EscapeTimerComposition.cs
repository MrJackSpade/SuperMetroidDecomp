namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// Names the escape timer's parts (<see cref="EscapeTimerPresentationDefinitions"/>): the glyph that draws a decimal
/// value and the value a glyph draws, each glyph's sheet frame, and each anchor's JSON key and stock position.
/// </summary>
public static class EscapeTimerComposition
{
    /// <summary>The glyph that draws the decimal value <paramref name="digit"/>.</summary>
    /// <param name="digit">Decoded decimal value 0 through 9, not a packed-BCD byte.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/> is outside 0..9.</exception>
    public static EscapeTimerGlyph DigitGlyph(int digit) => Ensure.Element(EscapeTimerPresentationDefinitions.DigitGlyphs, digit);

    /// <summary>The decimal value a digit glyph draws.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="glyph"/> is the label.</exception>
    public static int DigitValue(EscapeTimerGlyph glyph) => Ensure.Listed(EscapeTimerPresentationDefinitions.DigitValues, glyph);

    /// <summary>The sheet frame name of a glyph: Label, or Digit.0 through Digit.9.</summary>
    public static string FrameName(EscapeTimerGlyph glyph) => Ensure.Listed(EscapeTimerPresentationDefinitions.FrameNames, glyph);

    /// <summary>The JSON key of an anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    public static string AnchorName(EscapeTimerAnchor anchor) => Ensure.Listed(EscapeTimerPresentationDefinitions.AnchorNames, anchor);

    /// <summary>$80:9F7B/9F84/9F8D: three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static Offset16<SpritePixels> DefaultAnchor(EscapeTimerAnchor anchor) =>
        Ensure.Listed(EscapeTimerPresentationDefinitions.DefaultAnchors, anchor);
}

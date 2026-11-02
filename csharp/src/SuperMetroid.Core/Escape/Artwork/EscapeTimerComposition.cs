namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// Names the escape timer's parts (<see cref="EscapeTimerPresentationDefinitions"/>): the glyph that draws a decimal
/// value, each glyph's sheet frame, and each anchor's stock position.
/// </summary>
public static class EscapeTimerComposition
{
    /// <summary>The glyph that draws the decimal value <paramref name="digit"/>.</summary>
    /// <param name="digit">Decoded decimal value 0 through 9, not a packed-BCD byte.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/> is outside 0..9.</exception>
    public static EscapeTimerGlyph DigitGlyph(int digit) => Ensure.Element(EscapeTimerPresentationDefinitions.DigitGlyphs, digit);

    /// <summary>The sheet frame name of a glyph: Label, or Digit.0 through Digit.9.</summary>
    public static string FrameName(EscapeTimerGlyph glyph) => Ensure.Listed(EscapeTimerPresentationDefinitions.FrameNames, glyph);

    /// <summary>Three two-digit groups separated by one cell, centered across eight cells.</summary>
    internal static Offset16<SpritePixels> DefaultAnchor(EscapeTimerAnchor anchor) =>
        Ensure.Listed(EscapeTimerPresentationDefinitions.DefaultAnchors, anchor);
}

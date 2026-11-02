using SuperMetroid.Core;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// Names the escape timer layout document's parts (<see cref="EscapeTimerLayoutDefinitions"/>) for the import (#1502):
/// the value a digit glyph draws and each anchor's JSON key.
/// </summary>
public static class EscapeTimerLayoutNames
{
    /// <summary>The decimal value a digit glyph draws.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="glyph"/> is the label.</exception>
    public static int DigitValue(EscapeTimerGlyph glyph) => Ensure.Listed(EscapeTimerLayoutDefinitions.DigitValues, glyph);

    /// <summary>The JSON key of an anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    public static string AnchorName(EscapeTimerAnchor anchor) => Ensure.Listed(EscapeTimerLayoutDefinitions.AnchorNames, anchor);
}

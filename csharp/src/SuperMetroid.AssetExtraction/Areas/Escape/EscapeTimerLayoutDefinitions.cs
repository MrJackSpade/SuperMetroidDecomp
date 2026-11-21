using SuperMetroid.Core;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The escape timer's installed layout document as the import reads and writes it (#1502): its revision and file name,
/// the glyphs and anchors it names, each anchor's JSON key and the decimal value each digit glyph draws.
/// </summary>
public static partial class EscapeTimerLayoutDefinitions
{
    /// <summary>Supported JSON revision for the timer's pixel anchors and digit spacing; its frames are the installed sheet.</summary>
    public const int Version = 2;
    /// <summary>Installation-relative JSON layout filename, separate from the timer font's indexed PNG artwork.</summary>
    public const string FileName = "escape-timer.json";

    /// <summary>Every composition in the installed sheet's order (#2567): the TIME label, then the digits 0 through 9.</summary>
    public static IReadOnlyList<EscapeTimerGlyph> Glyphs { get; } = Ensure.EveryMemberOnce<EscapeTimerGlyph>(
    [
        EscapeTimerGlyph.Label,
        EscapeTimerGlyph.Digit0,
        EscapeTimerGlyph.Digit1,
        EscapeTimerGlyph.Digit2,
        EscapeTimerGlyph.Digit3,
        EscapeTimerGlyph.Digit4,
        EscapeTimerGlyph.Digit5,
        EscapeTimerGlyph.Digit6,
        EscapeTimerGlyph.Digit7,
        EscapeTimerGlyph.Digit8,
        EscapeTimerGlyph.Digit9,
    ]);
    /// <summary>Every anchor in the document's order (#2567): the label origin, then the tens-digit origins of the three time pairs.</summary>
    public static IReadOnlyList<EscapeTimerAnchor> Anchors { get; } = Ensure.EveryMemberOnce<EscapeTimerAnchor>(
    [
        EscapeTimerAnchor.Label,
        EscapeTimerAnchor.Minutes,
        EscapeTimerAnchor.Seconds,
        EscapeTimerAnchor.Centiseconds,
    ]);

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

    /// <summary>The JSON key of each anchor, whose coordinates are pixel offsets from the live timer origin.</summary>
    internal static readonly IReadOnlyDictionary<EscapeTimerAnchor, string> AnchorNames = new Dictionary<EscapeTimerAnchor, string>
    {
        [EscapeTimerAnchor.Label] = "Label",
        [EscapeTimerAnchor.Minutes] = "Minutes",
        [EscapeTimerAnchor.Seconds] = "Seconds",
        [EscapeTimerAnchor.Centiseconds] = "Centiseconds",
    };
}

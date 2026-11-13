using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the zoomed-out exploding-Zebes fade-out.</summary>
/// <remarks>
/// Issue #841 / #625: the fade's definition
/// sets colour index 480, then runs seven 20-byte
/// records of <c>8, colors[8], Wait</c>, then <c>Delete</c>
/// after 56 frames. All 17 control words match. The first color row is
/// <c>2003,0E9A,05F9,0596,0133,008E,0009,0005</c>. For frame <c>f</c>
/// (0..6), columns 1..7 equal that column's first BGR555 color with
/// <c>max(0, component - 5*f)</c> applied separately to red, green, and blue;
/// column 3's red loses one additional unit when <c>f &gt; 0</c>. Column 0
/// stays 8195 for frames 0 and 1, then becomes zero. This bounded
/// relationship matches all 56 original colors exactly; it describes the authored
/// data, not runtime color arithmetic. The presentation compiler still supplies
/// all 56 live colors, and this catalog supplies only the controls.
/// </remarks>
public static class ExplodingZebesFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.FadeOutZoomedOutExplodingZebes, ColorsPerFrame);
}

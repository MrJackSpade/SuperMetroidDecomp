namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the zoomed-out exploding-Zebes fade-out.</summary>
/// <remarks>
/// Issue #841 / #625: in the pinned NTSC J/U v1.0 ROM, definition <c>$8D:E1C4</c>
/// enters at <c>$8D:CAAA</c>: <c>SetColorIndex($01E0)</c>, seven 20-byte
/// records of <c>8, colors[8], Wait</c>, then <c>Delete</c> at <c>$CB3A</c>
/// after 56 frames. All 17 control words match. The first color row is
/// <c>2003,0E9A,05F9,0596,0133,008E,0009,0005</c>. For frame <c>f</c>
/// (0..6), columns 1..7 equal that column's first BGR555 color with
/// <c>max(0, component - 5*f)</c> applied separately to red, green, and blue;
/// column 3's red loses one additional unit when <c>f &gt; 0</c>. Column 0
/// stays <c>$2003</c> for frames 0 and 1, then becomes zero. This bounded
/// relationship matches all 56 ROM colors exactly; it describes the authored
/// data, not runtime color arithmetic. The presentation compiler still supplies
/// all 56 live colors, and this catalog supplies only the controls. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ExplodingZebesFadePaletteFxColorDefinitions
{

    /// <summary>The one-shot fade contains seven timed records.</summary>
    public const int FrameCount = 7;

    /// <summary>Each record writes eight live BGR555 colors.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(86, FrameCount, ColorsPerFrame);
}

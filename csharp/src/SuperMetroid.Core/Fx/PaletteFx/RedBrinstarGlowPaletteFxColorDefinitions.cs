namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for Red Brinstar's background-glow palette loop.</summary>
/// <remarks>
/// Palette-FX definition <c>$F77D</c> owns fourteen timed records at $8D:EED7-$EFF6.
/// Its 112 BGR555 colors remain live presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// </remarks>
public static class RedBrinstarGlowPaletteFxColorDefinitions
{

    /// <summary>Fourteen ten-frame records form one complete glow cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(410, FrameCount, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}

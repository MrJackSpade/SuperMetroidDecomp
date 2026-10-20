namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for Red Brinstar's background-glow palette loop.</summary>
/// <remarks>
/// Palette-FX definition <c>$F77D</c> owns fourteen timed records at $8D:EED7-$EFF6.
/// Its 112 BGR555 colors remain live presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// </remarks>
public static class RedBrinstarGlowPaletteFxColorDefinitions
{

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.Brinstar21, ColorsPerFrame);
}

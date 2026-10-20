namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's shared general-level red-flash loop.</summary>
/// <remarks>
/// Definitions <c>$FFD1</c> and <c>$FFD5</c> select different CGRAM destinations before
/// converging at <c>$F94D</c>. The 98 BGR555 words remain live presentation data; this
/// catalog owns entry routing, timing, the inline CGRAM skip, waits, and loop control.
/// </remarks>
public static class TourianEscapeSharedRedFlashPaletteFxColorDefinitions
{

    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.CommonTourian20Tourian40, ColorsPerFrame);
}

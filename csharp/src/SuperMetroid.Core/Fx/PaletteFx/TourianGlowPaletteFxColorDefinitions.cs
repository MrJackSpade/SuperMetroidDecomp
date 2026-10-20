namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's glowing block and red-orb palette loop.</summary>
/// <remarks>
/// Definitions <c>$F7A1</c> and <c>$F7A5</c> enter one shared eleven-record program.
/// Its 88 BGR555 colors remain presentation data; this catalog owns entry setup, the
/// slot-sensitive pre-instruction, inline CGRAM skips, timing, waits, and loop control.
/// Clone entry $8D:F62A selects CGRAM byte $00E8 and jumps to $F636;
/// live entry $F632 selects the same index and falls through. $F636
/// installs pre-instruction $F621, which deletes the owner when two later
/// palette-FX slots exist. Records f=0..10 begin at $F63A + 22*f and
/// last ten frames each: one live color, $C5A2 skip of three CGRAM
/// colors, seven live colors, then $C595 wait. The $C61E goto at $F72C
/// returns to $F63A after a 110-frame cycle; f=11 reaches control.
/// All 43 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class TourianGlowPaletteFxColorDefinitions
{

    /// <summary>Eight live BGR555 colors are written by each record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.CommonTourian2Tourian41, ColorsPerFrame);
}

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the shared Crateria/Brinstar beacon-flashing loop.</summary>
/// <remarks>
/// Definition <c>$F781</c> is used by pre-Tourian hall, Red Brinstar mainstreet, the
/// Red Brinstar elevator, and early Kraid rooms. Its sound-ID byte is a fixed
/// library-two selection; setup, timing, CGRAM skips, audio command, and loop
/// flow compile. The forty BGR555 colors remain presentation data.
/// $8D:EFF7 selects CGRAM byte $00E2. Ten records last ten frames each:
/// frames i=0..5 begin at $EFFB + 14*i; frames i=6..9 begin at
/// $F052 + 14*(i-6), after the $C673 library-two sound opcode at $F04F
/// consumes the compiled one-byte ID $18 at $F051. Each record writes three
/// colors, runs $C5BD to skip nine CGRAM colors, writes one more color,
/// then waits at $C595. The $C61E goto at $F08A returns to $EFFB;
/// frame ten reaches that control after a 100-frame cycle. All 35
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class BeaconPaletteFxColorDefinitions
{

    /// <summary>Ten ten-frame records form one complete flash cycle.</summary>
    public const int FrameCount = 10;

    /// <summary>Four live BGR555 colors are written by each timed record.</summary>
    public const int ColorsPerFrame = 4;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(424, FrameCount, ColorsPerFrame);
}

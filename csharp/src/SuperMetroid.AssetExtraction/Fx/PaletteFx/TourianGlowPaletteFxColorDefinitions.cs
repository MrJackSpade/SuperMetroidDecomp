using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's glowing block and red-orb palette loop.</summary>
/// <remarks>
/// The two definitions enter one shared eleven-record program.
/// Its 88 BGR555 colors remain presentation data; this catalog owns entry setup, the
/// slot-sensitive frame behaviour, inline colour skips, timing, waits, and loop control.
/// The clone entry selects colour byte 232 and joins the live entry,
/// which selects the same index. Both
/// install the frame behaviour that deletes the owner when two later
/// palette-FX slots exist. Records f=0..10 are 22 bytes apart and
/// last ten frames each: one live color, a skip of three
/// colors, seven live colors, then a wait. A final goto
/// returns to the first record after a 110-frame cycle; f=11 reaches control.
/// All 43 mechanics words match the original program.
/// </remarks>
public static class TourianGlowPaletteFxColorDefinitions
{

    /// <summary>Eight live BGR555 colors are written by each record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.CommonTourianGlowCycle, ColorsPerFrame);
}

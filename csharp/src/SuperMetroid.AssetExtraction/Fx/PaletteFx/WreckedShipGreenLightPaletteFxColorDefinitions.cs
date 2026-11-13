using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable control words for Wrecked Ship's powered green-light palette loop.
/// </summary>
/// <remarks>
/// The two palette-FX definitions share this program. Its sixteen
/// BGR555 colors remain live presentation data; only setup, timing, waits, and loop control
/// are compiled here.
/// The program selects colour byte 152. Records f=0..7 are
/// eight bytes apart, last ten frames, write two live colors, and end in
/// a wait. A final goto returns to the first record after an
/// 80-frame cycle; f=8 reaches control. All 20 mechanics words match
/// the original program for both definition entries.
/// </remarks>
public static class WreckedShipGreenLightPaletteFxColorDefinitions
{

    /// <summary>Two BGR555 colors are presentation-owned by each record.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.WreckedShipGreenLightsCycle, ColorsPerFrame);
}

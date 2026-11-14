using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the shared Crateria/Brinstar beacon-flashing loop.</summary>
/// <remarks>
/// The beacon definition is used by pre-Tourian hall, Red Brinstar mainstreet, the
/// Red Brinstar elevator, and early Kraid rooms. Its sound-ID byte is a fixed
/// library-two selection; setup, timing, colour skips, audio command, and loop
/// flow compile. The forty BGR555 colors remain presentation data.
/// Ten records last ten frames each; after the sixth record
/// the program queues the beacon sound.
/// Each record writes three
/// colors, skips nine colors, writes one more color,
/// then waits. A final goto returns to the first record;
/// frame ten reaches that control after a 100-frame cycle. All 35
/// mechanics words match the original program.
/// </remarks>
public static class BeaconPaletteFxColorDefinitions
{

    /// <summary>Four live BGR555 colors are written by each timed record.</summary>
    public const int ColorsPerFrame = 4;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectAnimation.BeaconFlashing, ColorsPerFrame);
}

using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the shared Crateria and Brinstar beacon flashing and the colours each writes, for the import.</summary>
/// <remarks>
/// The beacon flashing shows in the pre-Tourian hall, Red Brinstar's main street, the Red Brinstar elevator and the
/// early Kraid rooms.
/// </remarks>
public static class BeaconPaletteFxColorDefinitions
{

    /// <summary>Four live BGR555 colors are written by each timed record.</summary>
    internal const int ColorsPerFrame = 4;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.BeaconFlashing, ColorsPerFrame);
}

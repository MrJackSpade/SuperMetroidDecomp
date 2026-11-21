using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of the four Tourian entrance statues' grey-outs and the colours each writes, for the import.
/// </summary>
/// <remarks>
/// Draygon's, Kraid's, Ridley's and Phantoon's statues each grey out their own colours through one shared run of records.
/// </remarks>
public static partial class TourianStatueGreyPaletteFxColorDefinitions
{

    /// <summary>Eight colours are presentation-owned by each timed record.</summary>
    internal const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.GreyOutTourianStatuePhantoon, ColorsPerFrame);
}

using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of Wrecked Ship's powered green lights and the colours each writes, for the import.
/// </summary>
/// <remarks>
/// The two effects share these records.
/// </remarks>
public static class WreckedShipGreenLightPaletteFxColorDefinitions
{

    /// <summary>Two BGR555 colors are presentation-owned by each record.</summary>
    internal const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.WreckedShipGreenLights, ColorsPerFrame);
}

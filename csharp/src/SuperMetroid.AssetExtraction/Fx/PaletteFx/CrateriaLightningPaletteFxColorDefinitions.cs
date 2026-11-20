using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of the Landing Site's lightning and the colours each writes, for the import.
/// </summary>
public static class CrateriaLightningPaletteFxColorDefinitions
{
    /// <summary>The Landing Site program whose flash colors light eight of the background's colours.</summary>
    public static CrateriaLightningPaletteFxColorDefinition Surface { get; } =
        new()
        {
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.CrateriaLightning, 8),
        };
}

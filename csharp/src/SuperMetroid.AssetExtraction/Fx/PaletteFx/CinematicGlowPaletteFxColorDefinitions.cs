using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the old Mother Brain lights and the gunship glow and the colours each writes, for the import.</summary>
public static class CinematicGlowPaletteFxColorDefinitions
{


    /// <summary>
    /// The old-Mother-Brain and gunship glow programs in definition order: the three-color, six-frame light pulse,
    /// then the one-color, five-frame gunship glow.
    /// </summary>
    public static IReadOnlyList<CinematicGlowPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.OldMotherBrainFightBackgroundLights, 3),
        },
        new()
        {
            Owner = CinematicGlowPaletteFxProgramOwner.GunshipGlow,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.GunshipGlow, 1),
        },
    ];
}

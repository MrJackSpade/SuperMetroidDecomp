using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the cinematic PLANET ZEBES text fade-in and fade-out and the colours each writes, for the import.</summary>
/// <remarks>
/// The fade-out shows the fade-in's colours in reverse order.
/// </remarks>
public static class PlanetZebesTextPaletteFxColorDefinitions
{

    /// <summary>Three text colors per record.</summary>
    private const int ColorsPerFrame = 3;

    /// <summary>The fade-in and fade-out programs in definition order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } =
        [
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeIn,
                ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.FadeInPlanetZebesText, ColorsPerFrame),
            },
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeOut,
                ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.FadeOutPlanetZebesText, ColorsPerFrame),
            },
        ];

}

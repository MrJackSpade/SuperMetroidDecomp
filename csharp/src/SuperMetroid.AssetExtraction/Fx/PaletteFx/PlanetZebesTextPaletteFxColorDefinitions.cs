using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the cinematic PLANET ZEBES text fade-in and fade-out and the colours each writes, for the import.</summary>
/// <remarks>
/// The fade-out shows the fade-in's colours in reverse order.
/// </remarks>
public static class PlanetZebesTextPaletteFxColorDefinitions
{

    /// <summary>Three text colors per record.</summary>
    internal const int ColorsPerFrame = 3;

    /// <summary>The text's fade-in.</summary>
    internal static PlanetZebesTextPaletteFxColorDefinition FadeInPlanetZebesText { get; } = new(ColourEffectId.FadeInPlanetZebesText, ColorsPerFrame);

    /// <summary>The text's fade-out.</summary>
    internal static PlanetZebesTextPaletteFxColorDefinition FadeOutPlanetZebesText { get; } = new(ColourEffectId.FadeOutPlanetZebesText, ColorsPerFrame);

    /// <summary>The fade-in and fade-out programs in definition order.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } = [FadeInPlanetZebesText, FadeOutPlanetZebesText];

}

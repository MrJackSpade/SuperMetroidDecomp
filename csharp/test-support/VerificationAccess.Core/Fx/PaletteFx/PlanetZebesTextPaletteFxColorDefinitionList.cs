namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="PlanetZebesTextPaletteFxColorDefinitions"/> a check walks (#2604): the fade-in and fade-out programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class PlanetZebesTextPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="PlanetZebesTextPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All =>
            [PlanetZebesTextPaletteFxColorDefinitions.FadeInPlanetZebesText, PlanetZebesTextPaletteFxColorDefinitions.FadeOutPlanetZebesText];
    }
}

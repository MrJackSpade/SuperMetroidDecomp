namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="NorfairEnvironmentalPaletteFxColorDefinitions"/> a check walks (#2604): the heat-phase program, then the green, purple and dark foreground glows. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NorfairEnvironmentalPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All =>
        [
            NorfairEnvironmentalPaletteFxColorDefinitions.NorfairForegroundAndHeatPhase, NorfairEnvironmentalPaletteFxColorDefinitions.NorfairGreenForegroundHeatGlow,
            NorfairEnvironmentalPaletteFxColorDefinitions.NorfairPurpleForegroundHeatGlow, NorfairEnvironmentalPaletteFxColorDefinitions.NorfairDarkForegroundHeatGlow,
        ];
    }
}

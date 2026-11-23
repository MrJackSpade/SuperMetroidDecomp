namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="MaridiaEnvironmentalPaletteFxColorDefinitions"/> a check walks (#2604): the sand-pit, sand-fall, and waterfall programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class MaridiaEnvironmentalPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="MaridiaEnvironmentalPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<MaridiaEnvironmentalPaletteFxColorDefinition> All =>
        [
            MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaSandPits, MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaSandFalls,
            MaridiaEnvironmentalPaletteFxColorDefinitions.MaridiaBackgroundWaterfalls,
        ];
    }
}

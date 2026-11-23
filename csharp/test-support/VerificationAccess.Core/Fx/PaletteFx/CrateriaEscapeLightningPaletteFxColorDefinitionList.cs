namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="CrateriaEscapeLightningPaletteFxColorDefinitions"/> a check walks (#2604): the yellow-lightning and Parlor sky programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class CrateriaEscapeLightningPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="CrateriaEscapeLightningPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition> All =>
            [CrateriaEscapeLightningPaletteFxColorDefinitions.CrateriaEscapeYellowLightning, CrateriaEscapeLightningPaletteFxColorDefinitions.ParlorEscapeSkyFlicker];
    }
}

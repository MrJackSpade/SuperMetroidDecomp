namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="SamusLoadingSuitPaletteFxColorDefinitions"/> a check walks (#2604): the power, Varia, and gravity programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class SamusLoadingSuitPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="SamusLoadingSuitPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All =>
            [SamusLoadingSuitPaletteFxColorDefinitions.PowerSuit, SamusLoadingSuitPaletteFxColorDefinitions.VariaSuit, SamusLoadingSuitPaletteFxColorDefinitions.GravitySuit];
    }
}

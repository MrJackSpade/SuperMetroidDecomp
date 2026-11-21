namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="TorizoBellyPaletteFxColorDefinitions"/> a check walks (#2604): the Bomb and Golden Torizo programs in palette-definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class TorizoBellyPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(TorizoBellyPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TorizoBellyPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<TorizoBellyPaletteFxColorDefinition> All =>
            [TorizoBellyPaletteFxColorDefinitions.BombTorizoBelly, TorizoBellyPaletteFxColorDefinitions.GoldenTorizoBelly];
    }
}

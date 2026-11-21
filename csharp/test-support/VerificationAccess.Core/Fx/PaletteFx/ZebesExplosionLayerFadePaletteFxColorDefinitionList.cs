namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="ZebesExplosionLayerFadePaletteFxColorDefinitions"/> a check walks (#2604): the crust and grey-cloud one-shot fades in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class ZebesExplosionLayerFadePaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionLayerFadePaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<ZebesExplosionLayerFadePaletteFxColorDefinition> All =>
            [ZebesExplosionLayerFadePaletteFxColorDefinitions.FadeOutExplodingZebesCrust, ZebesExplosionLayerFadePaletteFxColorDefinitions.FadeOutExplodingZebesGreyClouds];
    }
}

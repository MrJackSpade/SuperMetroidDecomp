namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="ZebesExplosionAmbientPaletteFxColorDefinitions"/> a check walks (#2604): the afterglow and lava programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class ZebesExplosionAmbientPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionAmbientPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<ZebesExplosionAmbientPaletteFxColorDefinition> All =>
            [ZebesExplosionAmbientPaletteFxColorDefinitions.ZebesExplosionPlanetAfterglow, ZebesExplosionAmbientPaletteFxColorDefinitions.ExplodingZebesLava];
    }
}

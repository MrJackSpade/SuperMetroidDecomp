namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="CinematicGlowPaletteFxColorDefinitions"/> a check walks (#2604): the old-Mother-Brain and gunship glow programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class CinematicGlowPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(CinematicGlowPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="CinematicGlowPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<CinematicGlowPaletteFxColorDefinition> All =>
            [CinematicGlowPaletteFxColorDefinitions.OldMotherBrainFightBackgroundLights, CinematicGlowPaletteFxColorDefinitions.GunshipGlow];
    }
}

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="TourianEscapeRedFlashPaletteFxColorDefinitions"/> a check walks (#2604): the shutter and background red-flash programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class TourianEscapeRedFlashPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TourianEscapeRedFlashPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<TourianEscapeRedFlashPaletteFxColorDefinition> All =>
            [TourianEscapeRedFlashPaletteFxColorDefinitions.TourianShutterRedFlashing, TourianEscapeRedFlashPaletteFxColorDefinitions.TourianBackgroundRedFlashing];
    }
}

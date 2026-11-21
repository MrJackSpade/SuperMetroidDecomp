namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The programs of <see cref="OldTourianEscapeAccentPaletteFxColorDefinitions"/> a check walks (#2604): the orange-railing and yellow-panel programs in definition order. The game names
/// each program where it starts it and keeps no list of them, so the checks that cover the catalog list them here.
/// </summary>
internal static class OldTourianEscapeAccentPaletteFxColorDefinitionList
{
    /// <summary>The catalog's programs in definition order.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeAccentPaletteFxColorDefinitionList"/>
        internal static IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition> All =>
            [OldTourianEscapeAccentPaletteFxColorDefinitions.OldTourianBackgroundRailingsFlashYellow, OldTourianEscapeAccentPaletteFxColorDefinitions.OldTourianBackgroundPanelsFlashYellow];
    }
}

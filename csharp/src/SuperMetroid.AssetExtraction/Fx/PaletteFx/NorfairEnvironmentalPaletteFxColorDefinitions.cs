using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Norfair's four synchronized environmental effects and the colours each writes, for the import.</summary>
/// <remarks>
/// The four effects share the heat shimmer's phases, one record for each.
/// </remarks>
public static class NorfairEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>Each phase writes five live BGR555 colors.</summary>
    public const int ColorsPerFrame = 5;

    /// <summary>
    /// Each program's owner with its palette-FX object, in definition order: the heat-phase program, then the green, purple
    /// and dark foreground glows.
    /// </summary>
    internal static readonly IReadOnlyList<KeyValuePair<NorfairEnvironmentalPaletteOwner, ColourEffectId>> Programs =
    [
        KeyValuePair.Create(NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase, ColourEffectId.NorfairForegroundAndHeatPhase),
        KeyValuePair.Create(NorfairEnvironmentalPaletteOwner.GreenForeground, ColourEffectId.NorfairGreenForegroundHeatGlow),
        KeyValuePair.Create(NorfairEnvironmentalPaletteOwner.PurpleForeground, ColourEffectId.NorfairPurpleForegroundHeatGlow),
        KeyValuePair.Create(NorfairEnvironmentalPaletteOwner.DarkForeground, ColourEffectId.NorfairDarkForegroundHeatGlow),
    ];

    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All { get; } = new NorfairEnvironmentalPalettePrograms();
}

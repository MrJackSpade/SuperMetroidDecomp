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

    /// <summary>Each program's owner, in definition order: the heat-phase program, then the green, purple and dark foreground glows.</summary>
    internal static readonly NorfairEnvironmentalPaletteOwner[] Owners =
    [
        NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase,
        NorfairEnvironmentalPaletteOwner.GreenForeground,
        NorfairEnvironmentalPaletteOwner.PurpleForeground,
        NorfairEnvironmentalPaletteOwner.DarkForeground,
    ];
    /// <summary>The palette-FX object of each program, by owner.</summary>
    internal static readonly ColourEffectId[] Objects =
    [
        ColourEffectId.NorfairForegroundAndHeatPhase,
        ColourEffectId.NorfairGreenForegroundHeatGlow,
        ColourEffectId.NorfairPurpleForegroundHeatGlow,
        ColourEffectId.NorfairDarkForegroundHeatGlow,
    ];

    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All { get; } = new NorfairEnvironmentalPalettePrograms();
}

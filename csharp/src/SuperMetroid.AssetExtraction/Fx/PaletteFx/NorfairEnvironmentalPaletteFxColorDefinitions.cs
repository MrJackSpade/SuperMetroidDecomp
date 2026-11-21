using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Norfair's four synchronized environmental effects and the colours each writes, for the import.</summary>
/// <remarks>
/// The four effects share the heat shimmer's phases, one record for each.
/// </remarks>
public static class NorfairEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>Each phase writes five live BGR555 colors.</summary>
    private const int ColorsPerFrame = 5;

    /// <summary>The foreground colours that pulse with the heat phase.</summary>
    internal static NorfairEnvironmentalPaletteFxColorDefinition NorfairForegroundAndHeatPhase { get; } = new(ColourEffectId.NorfairForegroundAndHeatPhase, ColorsPerFrame);

    /// <summary>The green foreground's heat glow.</summary>
    internal static NorfairEnvironmentalPaletteFxColorDefinition NorfairGreenForegroundHeatGlow { get; } = new(ColourEffectId.NorfairGreenForegroundHeatGlow, ColorsPerFrame);

    /// <summary>The purple foreground's heat glow.</summary>
    internal static NorfairEnvironmentalPaletteFxColorDefinition NorfairPurpleForegroundHeatGlow { get; } = new(ColourEffectId.NorfairPurpleForegroundHeatGlow, ColorsPerFrame);

    /// <summary>The dark foreground's heat glow.</summary>
    internal static NorfairEnvironmentalPaletteFxColorDefinition NorfairDarkForegroundHeatGlow { get; } = new(ColourEffectId.NorfairDarkForegroundHeatGlow, ColorsPerFrame);

    /// <summary>The four environmental programs in definition order: the heat-phase program, then the green, purple and dark foreground glows.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All { get; } =
        [NorfairForegroundAndHeatPhase, NorfairGreenForegroundHeatGlow, NorfairPurpleForegroundHeatGlow, NorfairDarkForegroundHeatGlow];
}

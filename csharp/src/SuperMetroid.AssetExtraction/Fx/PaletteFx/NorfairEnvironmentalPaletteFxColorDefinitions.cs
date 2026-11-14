using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for Norfair's four synchronized environmental loops.</summary>
/// <remarks>
/// The four Norfair definitions share sixteen authored phases. Their 320 BGR555 words
/// remain presentation data. The first owner additionally publishes sixteen byte-sized
/// heat-phase operands consumed by the separate Samus-in-heat palette object.
/// The first definition's records i=0..15 publish phase byte i,
/// then a duration, three colors, a skip of four colors, two
/// colors, and a wait. The other definitions' records
/// omit the phase publication
/// and skip eight colors. Their colour byte indices are 106, 130, 162,
/// and 194 respectively. Each terminal goto returns to its
/// first record after a 116-frame cycle; i=16 reaches control. All 224
/// mechanics words and sixteen phase bytes match the
/// original programs and the guarded production caller.
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
    /// <summary>The program each object runs, by owner: the heat-phase program, then the green, purple and dark foreground glows.</summary>
    internal static readonly ColourEffectAnimation[] Programs =
    [
        ColourEffectAnimation.NorfairForegroundAndHeatPhase,
        ColourEffectAnimation.NorfairGreenForegroundHeatGlow,
        ColourEffectAnimation.NorfairPurpleForegroundHeatGlow,
        ColourEffectAnimation.NorfairDarkForegroundHeatGlow,
    ];

    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All { get; } = new NorfairEnvironmentalPalettePrograms();
}

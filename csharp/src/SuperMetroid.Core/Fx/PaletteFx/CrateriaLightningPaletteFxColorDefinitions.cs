namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words and byte operands for Crateria's two lightning programs.
/// </summary>
/// <remarks>
/// Definition <c>$F765</c> is the live Landing Site lightning effect. Definition
/// <c>$F769</c> is the cartridge's unused dark-lightning counterpart. Their 202 BGR555
/// words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxColorDefinitions
{
    /// <summary>The Landing Site program whose flash colors target BG palette 5.</summary>
    private static readonly CrateriaLightningPaletteFxColorDefinition Surface = new(CrateriaLightningPaletteOwner.SurfaceLightning);
    /// <summary>The unused counterpart whose dark colors target CGRAM colors 65 through 71.</summary>
    private static readonly CrateriaLightningPaletteFxColorDefinition Dark = new(CrateriaLightningPaletteOwner.UnusedDarkLightning);
    /// <summary>The live and unused-dark programs in cartridge definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxColorDefinition> All { get; } = [Surface, Dark];
    /// <summary>$8D:EC59/$ED84 restart the neutral record when Samus is above Y=$0380.</summary>
    public const ushort VerticalSwitchSamusY = 0x0380;

}

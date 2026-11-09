namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words and byte operands for Crateria's two lightning programs.
/// </summary>
/// <remarks>
/// The first definition is the live Landing Site lightning effect. The second
/// is its unused dark-lightning counterpart. Their 202 colour
/// words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxColorDefinitions
{
    /// <summary>The Landing Site program whose flash colors light eight of the background's colours.</summary>
    private static readonly CrateriaLightningPaletteFxColorDefinition Surface =
        new()
        {
            Owner = CrateriaLightningPaletteOwner.SurfaceLightning,
            ColorsPerFrame = 8,
            NeutralCount = 1,
            ColorFrames = new(FirstRecord: 355, FrameCount: 12 + 1, ColorsPerFrame: 8),
        };
    /// <summary>The unused counterpart whose dark colors target colours 65 through 71.</summary>
    private static readonly CrateriaLightningPaletteFxColorDefinition Dark =
        new()
        {
            Owner = CrateriaLightningPaletteOwner.UnusedDarkLightning,
            ColorsPerFrame = 7,
            NeutralCount = 2,
            ColorFrames = new(FirstRecord: 368, FrameCount: 12 + 2, ColorsPerFrame: 7),
        };
    /// <summary>The live and unused-dark programs in definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxColorDefinition> All { get; } = [Surface, Dark];
    /// <summary>The room height Samus must stand above for both lightning programs to stay calm (<see cref="ColourEffectCondition.RestartsWhileSamusIsHigh"/>).</summary>
    public const int SamusHighOnSurfaceHeight = 896;

}

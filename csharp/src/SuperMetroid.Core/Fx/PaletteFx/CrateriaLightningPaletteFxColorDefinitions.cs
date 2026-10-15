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
    /// <summary>Stable view of both programs in cartridge definition order.</summary>
    private static readonly ProgramList Programs = new();

    /// <summary>The live and unused-dark programs in cartridge definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxColorDefinition> All => Programs;
    /// <summary>$8D:EC59/$ED84 restart the neutral record when Samus is above Y=$0380.</summary>
    public const ushort VerticalSwitchSamusY = 0x0380;

    /// <summary>Exposes the two retained lightning programs without allocating a collection.</summary>
    private sealed class ProgramList : IReadOnlyList<CrateriaLightningPaletteFxColorDefinition>
    {
        /// <summary>The surface program and its unused dark counterpart.</summary>
        public int Count => 2;
        /// <summary>Selects surface first, then dark; rejects any other ordinal.</summary>
        public CrateriaLightningPaletteFxColorDefinition this[int index] => index switch
        {
            0 => Surface,
            1 => Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates the live surface definition before the unused dark definition.</summary>
        public IEnumerator<CrateriaLightningPaletteFxColorDefinition> GetEnumerator()
        { yield return Surface; yield return Dark; }
        /// <summary>Adapts the same two-program sequence for non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

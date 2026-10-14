namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words and byte operands for Crateria's two lightning programs.
/// </summary>
/// <remarks>
/// Definition <c>$F765</c> is the live Landing Site lightning effect. Definition
/// <c>$F769</c> is the cartridge's unused dark-lightning counterpart. Their 202 BGR555
/// words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxProgramMechanicsDefinitions
{
    /// <summary>The Landing Site program whose flash colors target BG palette 5.</summary>
    private static readonly CrateriaLightningPaletteFxProgramDefinition Surface = new(CrateriaLightningPaletteOwner.SurfaceLightning);
    /// <summary>The unused counterpart whose dark colors target CGRAM colors 65 through 71.</summary>
    private static readonly CrateriaLightningPaletteFxProgramDefinition Dark = new(CrateriaLightningPaletteOwner.UnusedDarkLightning);
    /// <summary>Stable view of both programs in cartridge definition order.</summary>
    private static readonly ProgramList Programs = new();

    /// <summary>The live and unused-dark programs in cartridge definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition> All => Programs;
    /// <summary>$8D:EC59/$ED84 restart the neutral record when Samus is above Y=$0380.</summary>
    public const ushort VerticalSwitchSamusY = 0x0380;

    /// <summary>Resolves a compiled control or duration word from either lightning program, excluding their editable color payloads.</summary>
    /// <param name="pointer">Bank-$8D byte address; lookup requires an exact word-start identity, not general word alignment.</param>
    /// <param name="value">Compiled word on success, or zero when the address is not owned by either program's word mechanics.</param>
    /// <returns>Whether the address identifies a known instruction, operand, or duration word.</returns>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        Surface.TryReadWord(pointer, out value) || Dark.TryReadWord(pointer, out value);
    /// <summary>Resolves the byte-sized loop-timer operands of either lightning program: two repetitions for the first flash group and one for the final group.</summary>
    /// <param name="pointer">Exact bank-$8D byte address immediately after a set-timer instruction.</param>
    /// <param name="value">Timer value, two or one, on success; otherwise zero.</param>
    /// <returns>Whether the address identifies one of the four compiled timer operands across both programs.</returns>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value) =>
        Surface.TryReadByte(pointer, out value) || Dark.TryReadByte(pointer, out value);

    /// <summary>Exposes the two retained lightning programs without allocating a collection.</summary>
    private sealed class ProgramList : IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition>
    {
        /// <summary>The surface program and its unused dark counterpart.</summary>
        public int Count => 2;
        /// <summary>Selects surface first, then dark; rejects any other ordinal.</summary>
        public CrateriaLightningPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => Surface,
            1 => Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates the live surface definition before the unused dark definition.</summary>
        public IEnumerator<CrateriaLightningPaletteFxProgramDefinition> GetEnumerator()
        { yield return Surface; yield return Dark; }
        /// <summary>Adapts the same two-program sequence for non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

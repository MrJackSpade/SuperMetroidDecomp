namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed health-band policy and cartridge presentation addresses for Botwoon's palette
/// handler. Color words remain ROM-backed; only the phase/threshold decision is compiled.
/// </summary>
internal static class BotwoonHealthPaletteDefinitions
{
    /// <summary><c>BotwoonHealthBasedPalettes</c> at $B3:971B: eight 16-color records.</summary>
    public const int NativePaletteAddress = 0xb3971b;

    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    /// <summary>Terminal native byte offset after all eight threshold words are consumed.</summary>
    public const ushort CompletePhaseByteOffset = 16;

    /// <summary>$B3:981B, BotwoonHealthThresholdsForPaletteChange, eight palette-phase thresholds.</summary>
    /// <remarks>
    /// Issues #625 and #933 exact arithmetic: threshold(i)=375*(8-i), i=0..7; the public
    /// phase input is the even byte offset 2*i. Preserve the current sixteen-bit
    /// subtraction followed by SIGNED comparison, not an unsigned health comparison.
    /// LookupTableResearch checks all eight native words and all 8*65,536 phase/health
    /// pairs against ShouldAdvance, independently using ROM thresholds verified with
    /// pinned bank_B3.asm. Odd offsets and the terminal offset 16 remain invalid;
    /// the caller, not an extrapolated ninth threshold, owns completion.
    /// </remarks>
    private static ReadOnlySpan<ushort> Thresholds =>
        [3000, 2625, 2250, 1875, 1500, 1125, 750, 375];

    /// <summary>
    /// Returns whether the current health is below the threshold selected by an even native
    /// byte offset $00..$0E. The signed subtraction matches the cartridge's CMP/BPL result.
    /// </summary>
    public static bool ShouldAdvance(ushort phaseByteOffset, ushort health)
    {
        if ((phaseByteOffset & 1) != 0 || phaseByteOffset >= CompletePhaseByteOffset)
        {
            throw new InvalidDataException(
                $"Botwoon palette phase offset ${phaseByteOffset:X4} is outside the " +
                "eight even retail threshold entries.");
        }

        ushort threshold = Thresholds[phaseByteOffset / sizeof(ushort)];
        return unchecked((short)(health - threshold)) < 0;
    }
}

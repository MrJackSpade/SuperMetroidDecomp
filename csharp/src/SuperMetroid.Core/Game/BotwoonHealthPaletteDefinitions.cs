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

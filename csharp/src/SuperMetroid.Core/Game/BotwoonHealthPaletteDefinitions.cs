namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed health-band policy and cartridge presentation addresses for Botwoon's palette
/// handler. The eight authored color images are independently extractable visual assets.
/// </summary>
internal static class BotwoonHealthPaletteDefinitions
{
    /// <summary><c>BotwoonHealthBasedPalettes</c> at $B3:971B: eight 16-color records.</summary>
    public const int NativePaletteAddress = 0xb3971b;

    /// <summary>Eight complete sixteen-color palette images at $B3:971B-$981A.</summary>
    public const int PaletteCount = 8;
    public const int ColorsPerPalette = 16;

    /// <summary>Retail Botwoon writes sprite palette seven, CGRAM colors $F0-$FF.</summary>
    public const int DestinationColor = 240;

    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    /// <summary>Terminal native byte offset after all eight threshold words are consumed.</summary>
    public const ushort CompletePhaseByteOffset = 16;

    /// <summary>
    /// Returns whether the current health is below the threshold selected by an even native
    /// byte offset $00..$0E. The signed subtraction matches the cartridge's CMP/BPL result.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_B3.asm:
    /// $B3:981B has threshold(i)=375*(8-i), i=0..7. Divide the validated even byte
    /// offset by two before multiplying; subtract in sixteen bits and interpret the
    /// result as signed, matching CMP/BPL at $B3:983B. All ushort health values remain
    /// supported, including wrapped differences. Offset 16 is caller-owned completion,
    /// not an extrapolated threshold; odd offsets remain invalid.
    /// </remarks>
    public static bool ShouldAdvance(ushort phaseByteOffset, ushort health)
    {
        if ((phaseByteOffset & 1) != 0 || phaseByteOffset >= CompletePhaseByteOffset)
        {
            throw new InvalidDataException(
                $"Botwoon palette phase offset ${phaseByteOffset:X4} is outside the " +
                "eight even retail threshold entries.");
        }

        int threshold = 375 * (8 - phaseByteOffset / sizeof(ushort));
        return unchecked((short)(health - threshold)) < 0;
    }
}

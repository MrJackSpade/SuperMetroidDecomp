namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Zoa's shooting and rising programs.</summary>
/// <remarks>
/// Speed callbacks, durations, and loop control are immutable simulation data. The twelve
/// interleaved spritemap positions remain separate from mechanics word ownership.
/// </remarks>
internal abstract class ZoaInstructionProgramDefinitions
{
    /// <summary><c>$A3:B3C1</c>, left-facing horizontal launch.</summary>
    internal const ushort FacingLeftShooting = 0xb3c1;

    /// <summary><c>$A3:B3D7</c>, left-facing vertical rise.</summary>
    internal const ushort FacingLeftRising = 0xb3d7;

    /// <summary><c>$A3:B3E7</c>, right-facing horizontal launch.</summary>
    internal const ushort FacingRightShooting = 0xb3e7;

    /// <summary><c>$A3:B3FD</c>, right-facing vertical rise.</summary>
    internal const ushort FacingRightRising = 0xb3fd;

    /// <summary>$A3:B429 Instruction_Zoa_SetXSpeedTableIndexTo4, first shooting-stage callback.</summary>
    private const ushort FirstShot = 0xb429;
    /// <summary>$A3:B434 Instruction_Zoa_SetXSpeedTableIndexTo8, second shooting-stage callback.</summary>
    private const ushort SecondShot = 0xb434;
    /// <summary>$A3:B43F Instruction_Zoa_SetXSpeedTableIndexToC, final shooting-stage callback.</summary>
    private const ushort ThirdShot = 0xb43f;
    /// <summary>$A3:80ED Instruction_Common_GotoY, loops each program to its start.</summary>
    private const ushort Loop = 0x80ed;

    /// <summary>Reads the native shooting/rising instruction fields for either facing.
    /// Named speed callbacks, frame durations and loop targets replace the stored
    /// address/value records. All 26 words match the supported NTSC programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int start = address < FacingRightShooting ? FacingLeftShooting : FacingRightShooting;
        return (address - start) switch
        {
            0 => FirstShot,
            2 => 64,
            6 => SecondShot,
            8 => 8,
            12 => ThirdShot,
            14 => 48,
            18 or 34 => Loop,
            20 => (ushort)start,
            22 or 26 or 30 => 4,
            36 => (ushort)(start + 22),
            _ => throw new InvalidDataException(
                $"Zoa instruction mechanics pointer $A3:{address:X4} is not compiled."),
        };
    }
}

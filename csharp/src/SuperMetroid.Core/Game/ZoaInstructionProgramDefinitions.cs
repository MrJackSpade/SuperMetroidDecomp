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

    /// <summary>Reads the native shooting/rising instruction fields for either facing.
    /// Named speed callbacks, frame durations and loop targets replace the stored
    /// address/value records. All 26 words match the supported NTSC programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int start = address < FacingRightShooting ? FacingLeftShooting : FacingRightShooting;
        return (address - start) switch
        {
            0 => (ushort)ZoaInstruction.SetXSpeedTableIndexTo4,
            2 => 64,
            6 => (ushort)ZoaInstruction.SetXSpeedTableIndexTo8,
            8 => 8,
            12 => (ushort)ZoaInstruction.SetXSpeedTableIndexToC,
            14 => 48,
            18 or 34 => (ushort)CommonEnemyInstruction.Goto,
            20 => (ushort)start,
            22 or 26 or 30 => 4,
            36 => (ushort)(start + 22),
            _ => throw new InvalidDataException(
                $"Zoa instruction mechanics pointer $A3:{address:X4} is not compiled."),
        };
    }
}

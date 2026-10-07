namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Zoa's shooting and rising programs.</summary>
/// <remarks>
/// Speed callbacks, durations, and loop control are immutable simulation data. The twelve
/// interleaved spritemap positions remain separate from mechanics word ownership.
/// </remarks>
internal abstract class ZoaInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
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

    public static int MechanicsWordCount => 26;
    public static int PresentationWordCount => 12;

    /// <summary>Enumerates the thirteen mechanics words per facing in native address order.
    /// Shooting has three callback/timer pairs and a loop pair; rising has three
    /// timers and a loop pair. Presentation words stay interleaved and separate.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int start = index < 13 ? FacingLeftShooting : FacingRightShooting;
        int field = index % 13;
        int offset = field < 8 ? 6 * (field / 2) + 2 * (field % 2)
            : field < 11 ? 22 + 4 * (field - 8) : 34 + 2 * (field - 11);
        ushort address = (ushort)(start + offset);
        return new(address, ReadMechanicsWord(address));
    }

    /// <summary>Native sprite-pointer positions: three shooting frames at six-byte
    /// stride followed by three rising frames at four-byte stride, for each facing.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int start = index < 6 ? FacingLeftShooting : FacingRightShooting;
        int frame = index % 6;
        return (ushort)(start + (frame < 3 ? 4 + 6 * frame : 24 + 4 * (frame - 3)));
    }

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

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int relative = (address & 0xffff) - FacingLeftShooting;
        if ((uint)relative >= 76) return false;
        int offset = relative % 38;
        // Exclude the two presentation bytes in each timed frame. Both bytes of
        // callbacks, durations and loop instructions/targets belong to mechanics.
        return offset < 18 ? offset % 6 < 4
            : offset < 22 || offset >= 34 || (offset - 22) % 4 < 2;
    }
}

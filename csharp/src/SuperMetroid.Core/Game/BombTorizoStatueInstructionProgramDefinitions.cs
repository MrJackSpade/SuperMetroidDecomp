namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the sixteen Bomb Torizo statue-fragment programs at
/// $86:A4C3-$A5D3. Their thirty-two interleaved spritemap operands use extracted
/// presentation art. The packed sound-ID byte has its own typed sound decoder.
/// </summary>
internal abstract class BombTorizoStatueInstructionProgramDefinitions
{
    /// <summary>First program, <c>InstList_EnemyProjectile_BombTorizoChozoBreaking_Index0</c>, at $86:A4C3.</summary>
    internal const ushort FirstProgram = 0xa4c3;
    /// <summary>Byte distance between consecutive fragment programs.</summary>
    internal const ushort ProgramStride = 0x0011;
    /// <summary>Number of authored fragment programs selected by even parameters $00-$1E.</summary>
    internal const int ProgramCount = 16;

    /// <summary>$86:A4C7 and each subsequent fragment program's +4 command: library 2, sound $0C, Max6, immediately before falling starts.</summary>
    internal static EnemySoundRequest ReleaseSound(ushort commandAddress)
    {
        if (!TryDecodeProgramOffset(commandAddress, out _, out int offset) || offset != 4)
            throw new InvalidDataException($"Invalid statue-fragment sound command $86:{commandAddress:X4}.");
        return new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0c), 6);
    }

    /// <summary>Staggers fragment release by eight ticks per program until the sixty-four-tick floor.</summary>
    private static ushort InitialDuration(int programIndex) => (ushort)Math.Max(64, 128 - 8 * programIndex);
    /// <summary>Each of sixteen fragment programs has an initial and falling spritemap operand.</summary>
    public static int PresentationWordCount => ProgramCount * 2;

    /// <summary>Locates one of sixteen fragment programs selected by even native parameters.</summary>
    internal static ushort Program(int index)
    {
        if ((uint)index >= ProgramCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return unchecked((ushort)(FirstProgram + index * ProgramStride));
    }
    /// <summary>Locates the initial or falling sprite operand within a fragment program.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        ushort program = Program(index / 2);
        return unchecked((ushort)(program + ((index & 1) == 0 ? 2 : 13)));
    }

    /// <summary>Separates the two visual operands from a fragment program's control words.</summary>
    internal static bool IsPresentationWord(ushort address) =>
        TryDecodeProgramOffset(address, out _, out int offset) && offset is 2 or 13;
    /// <summary>Compiles staggered release, sound, falling pre-instruction, hold, and deletion words.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!TryDecodeProgramOffset(address, out int programIndex, out int offset))
        {
            throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled.");
        }

        return offset switch
        {
            0 => InitialDuration(programIndex),
            4 => (ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6,
            7 => (ushort)EnemyProjectileInstruction.PreInstructionInY,
            9 => (ushort)EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling,
            11 => 0x0070,
            15 => (ushort)EnemyProjectileInstruction.Delete,
            _ => throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled."),
        };
    }

    /// <summary>Maps a bank-$86 address to its fragment ordinal and byte offset within the seventeen-byte record.</summary>
    internal static bool TryDecodeProgramOffset(
        ushort address,
        out int programIndex,
        out int offset)
    {
        int relative = address - FirstProgram;
        if ((uint)relative >= ProgramCount * ProgramStride)
        {
            programIndex = 0;
            offset = 0;
            return false;
        }

        programIndex = relative / ProgramStride;
        offset = relative % ProgramStride;
        return true;
    }
}

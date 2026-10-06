namespace SuperMetroid.Core.Game;

internal readonly record struct BombTorizoStatueInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the sixteen Bomb Torizo statue-fragment programs at
/// $86:A4C3-$A5D3. Their thirty-two interleaved spritemap operands use extracted
/// presentation art. The packed sound-ID byte has its own typed sound decoder.
/// </summary>
internal static class BombTorizoStatueInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProj_BombTorizoChozoBreaking</c> at $86:A764.</summary>
    internal const ushort InitializationAi = 0xa764;
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

    // Fragment release delays decrease by eight ticks, with a sixty-four-tick floor.
    private static ushort InitialDuration(int programIndex) => (ushort)Math.Max(64, 128 - 8 * programIndex);

    internal static int MechanicsWordCount => ProgramCount * 6;
    internal static int PresentationWordCount => ProgramCount * 2;

    internal static ushort Program(int index)
    {
        if ((uint)index >= ProgramCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return unchecked((ushort)(FirstProgram + index * ProgramStride));
    }

    internal static BombTorizoStatueInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int programIndex = index / 6;
        ushort program = Program(programIndex);
        int word = index % 6;
        // First timed frame, three-byte sound command, callback plus argument,
        // second timed frame, then delete. Reuse the direct reader for values.
        int offset = word switch
        {
            0 => 0,
            1 => 4,
            2 or 3 or 4 => 7 + 2 * (word - 2),
            _ => 15,
        };
        ushort address = (ushort)(program + offset);
        return new(address, ReadMechanicsWord(address));
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        ushort program = Program(index / 2);
        return unchecked((ushort)(program + ((index & 1) == 0 ? 2 : 13)));
    }

    internal static bool IsPresentationWord(ushort address) =>
        TryDecodeProgramOffset(address, out _, out int offset) && offset is 2 or 13;
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
            4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6,
            7 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
            9 => EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling,
            11 => 0x0070,
            15 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            _ => throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled."),
        };
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        return IsMechanicsWordStart(bankAddress) ||
            IsMechanicsWordStart(unchecked((ushort)(bankAddress - 1)));
    }

    private static bool IsMechanicsWordStart(ushort address) =>
        TryDecodeProgramOffset(address, out _, out int offset) &&
        offset is 0 or 4 or 7 or 9 or 11 or 15;

    private static bool TryDecodeProgramOffset(
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

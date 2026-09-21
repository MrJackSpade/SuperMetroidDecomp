namespace SuperMetroid.Core.Game;

internal readonly record struct BombTorizoStatueInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the sixteen Bomb Torizo statue-fragment programs at
/// $86:A4C3-$A5D3. Their thirty-two interleaved spritemap operands and sixteen packed
/// sound IDs remain live cartridge presentation/audio data.
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

    private static readonly ushort[] InitialDurations =
    [
        0x0080, 0x0078, 0x0070, 0x0068,
        0x0060, 0x0058, 0x0050, 0x0048,
        0x0040, 0x0040, 0x0040, 0x0040,
        0x0040, 0x0040, 0x0040, 0x0040,
    ];

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
        return (index % 6) switch
        {
            0 => new BombTorizoStatueInstructionMechanicsWord(
                program, InitialDurations[programIndex]),
            1 => new BombTorizoStatueInstructionMechanicsWord(
                unchecked((ushort)(program + 4)),
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6),
            2 => new BombTorizoStatueInstructionMechanicsWord(
                unchecked((ushort)(program + 7)),
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
            3 => new BombTorizoStatueInstructionMechanicsWord(
                unchecked((ushort)(program + 9)),
                EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling),
            4 => new BombTorizoStatueInstructionMechanicsWord(
                unchecked((ushort)(program + 11)), 0x0070),
            _ => new BombTorizoStatueInstructionMechanicsWord(
                unchecked((ushort)(program + 15)),
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        };
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        ushort program = Program(index / 2);
        return unchecked((ushort)(program + ((index & 1) == 0 ? 2 : 13)));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!TryDecodeProgramOffset(address, out int programIndex, out int offset))
        {
            throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled.");
        }

        return offset switch
        {
            0 => InitialDurations[programIndex],
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

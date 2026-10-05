namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kago-bug projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct KagoBugProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Kago bug's landed, falling, jumping, and shot programs. The
/// shared initial Kraid-rock pose is owned by
/// <see cref="KraidRockProjectileInstructionProgramDefinitions"/>; interleaved
/// spritemap operands select compiled presentation identities.
/// </summary>
internal static class KagoBugProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_KagoBug_HitFloor</c> at $86:D03C.</summary>
    internal const ushort Landed = 0xd03c;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Falling</c> at $86:D04A.</summary>
    internal const ushort Falling = 0xd04a;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_0</c> at $86:D052.</summary>
    internal const ushort JumpStart = 0xd052;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_1</c> at $86:D05C.</summary>
    internal const ushort JumpLoop = 0xd05c;

    /// <summary><c>InstList_EnemyProjectile_Shot_KagoBug</c> at $86:D064.</summary>
    internal const ushort Shot = 0xd064;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartJumping</c> at $86:D15C.
    /// </summary>
    internal const ushort StartJumpInstruction = 0xd15c;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartIdling</c> at $86:D1B6.
    /// </summary>
    internal const ushort StartIdleInstruction = 0xd1b6;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_UsePalette0_duplicate_again</c> at $86:D1C7.
    /// </summary>
    internal const ushort UsePaletteZeroInstruction = 0xd1c7;

    /// <summary>
    /// <c>PreInstruction_EnemyProjectile_KagoBug_SpawnDrop</c> used as an instruction
    /// callback at $86:D1CE.
    /// </summary>
    internal const ushort SpawnDropInstruction = 0xd1ce;

    internal static int MechanicsWordCount => 23;
    internal static int PresentationWordCount => 11;
    internal static KagoBugProjectileInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index is >= 5 and < 8) return HoldLoop(Falling, index - 5);
        if (index is >= 11 and < 14) return HoldLoop(JumpLoop, index - 11);
        if (index is >= 15 and < 20) return new((ushort)(Shot + 2 + 4 * (index - 15)), 4);
        return index switch
        {
            0 => new(Landed, 5),
            1 => new(Landed + 4, StartIdleInstruction),
            2 => new(Landed + 6, 0x7fff),
            3 => new(Landed + 10, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            4 => new(Landed + 12, Landed),
            8 => new(JumpStart, 16),
            9 => new(JumpStart + 4, 5),
            10 => new(JumpStart + 8, StartJumpInstruction),
            14 => new(Shot, UsePaletteZeroInstruction),
            20 => new(Shot + 22, SpawnDropInstruction),
            21 => new(Shot + 24, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            _ => new(Shot + 26, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        };
    }

    private static KagoBugProjectileInstructionMechanicsWord HoldLoop(ushort start, int index) => index switch
    {
        0 => new(start, 0x7fff),
        1 => new((ushort)(start + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        _ => new((ushort)(start + 6), start),
    };

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index >= 6) return (ushort)(Shot + 4 + 4 * (index - 6));
        return index switch
        {
            0 => Landed + 2,
            1 => Landed + 8,
            2 => Falling + 2,
            3 => JumpStart + 2,
            4 => JumpStart + 6,
            _ => JumpLoop + 2,
        };
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KagoBugProjectileInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Kago-bug projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

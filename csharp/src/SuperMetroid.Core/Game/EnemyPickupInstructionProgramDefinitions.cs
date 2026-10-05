namespace SuperMetroid.Core.Game;

/// <summary>One compiled enemy-pickup mechanics word at its bank-$86 address.</summary>
internal readonly record struct EnemyPickupInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the five live enemy-pickup animation programs. Their sixteen
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal static class EnemyPickupInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pickup_SmallEnergy</c> at $86:ED8D.</summary>
    internal const ushort SmallEnergy = 0xed8d;

    /// <summary><c>InstList_EnemyProjectile_Pickup_BigEnergy</c> at $86:EDA3.</summary>
    internal const ushort BigEnergy = 0xeda3;

    /// <summary><c>InstList_EnemyProjectile_Pickup_Missiles</c> at $86:EDB9.</summary>
    internal const ushort Missiles = 0xedb9;

    /// <summary><c>InstList_EnemyProjectile_Pickup_SuperMissiles</c> at $86:EDDD.</summary>
    internal const ushort SuperMissiles = 0xeddd;

    /// <summary><c>InstList_EnemyProjectile_Pickup_PowerBombs</c> at $86:EDEB.</summary>
    internal const ushort PowerBombs = 0xedeb;

    internal static int MechanicsWordCount => 30;
    internal static int PresentationWordCount => 16;

    /// <summary>
    /// Energy pickups display four frames for eight ticks each. Missiles display
    /// two frames and Power Bombs four, for five ticks each. Every program loops;
    /// the four earlier programs also have an unreachable trailing sleep command.
    /// </summary>
    private static PickupLoop ProgramAt(int program) => program switch
    {
        0 => new(SmallEnergy, 4, 8, true),
        1 => new(BigEnergy, 4, 8, true),
        2 => new(Missiles, 2, 5, true),
        3 => new(SuperMissiles, 2, 5, true),
        4 => new(PowerBombs, 4, 5, false),
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    };

    private readonly record struct PickupLoop(ushort Start, int Frames, ushort Duration, bool HasSleep)
    {
        internal int MechanicsWords => Frames + (HasSleep ? 3 : 2);
        internal EnemyPickupInstructionMechanicsWord Word(int index)
        {
            if (index < Frames)
                return new((ushort)(Start + 4 * index), Duration);
            int command = index - Frames;
            return new((ushort)(Start + 4 * Frames + 2 * command), command switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
                1 => Start,
                _ => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
            });
        }
    }

    internal static EnemyPickupInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 5; program++)
        {
            PickupLoop loop = ProgramAt(program);
            if (index < loop.MechanicsWords)
                return loop.Word(index);
            index -= loop.MechanicsWords;
        }
        throw new IndexOutOfRangeException();
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 5; program++)
        {
            PickupLoop loop = ProgramAt(program);
            if (index < loop.Frames)
                return (ushort)(loop.Start + 4 * index + 2);
            index -= loop.Frames;
        }
        throw new IndexOutOfRangeException();
    }

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind is RoomEnemyProjectileKind.EnemyDeathPickup or
            RoomEnemyProjectileKind.EnemyDeathExplosion) &&
        TryRead(address, out _);

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Enemy-pickup mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return TryRead(bankAddress, out _) || TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }

    private static bool TryRead(ushort address, out ushort value)
    {
        for (int program = 0; program < 5; program++)
        {
            PickupLoop loop = ProgramAt(program);
            int offset = address - loop.Start;
            if (offset < 0)
                continue;
            if (offset < loop.Frames * 4)
            {
                if ((offset & 3) == 0)
                {
                    value = loop.Duration;
                    return true;
                }
                continue;
            }
            int command = offset - loop.Frames * 4;
            if (command == 0 || command == 2 || (command == 4 && loop.HasSleep))
            {
                value = loop.Word(loop.Frames + command / 2).Value;
                return true;
            }
        }
        value = 0;
        return false;
    }
}
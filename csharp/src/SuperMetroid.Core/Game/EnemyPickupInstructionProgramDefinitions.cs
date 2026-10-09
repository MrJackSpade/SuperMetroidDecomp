namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the five live enemy-pickup animation programs. Their sixteen
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class EnemyPickupInstructionProgramDefinitions
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

    /// <summary>Total number of compiled frame spritemap operands across all five pickup animation programs.</summary>
    public static int PresentationWordCount => 16;

    /// <summary>
    /// Energy pickups display four frames for eight ticks each. Missiles display
    /// two frames and Power Bombs four, for five ticks each. Every program loops;
    /// the four earlier programs also have an unreachable trailing sleep command.
    /// </summary>
    internal static PickupLoop ProgramAt(int program) => program switch
    {
        0 => new(SmallEnergy, 4, 8, true),
        1 => new(BigEnergy, 4, 8, true),
        2 => new(Missiles, 2, 5, true),
        3 => new(SuperMissiles, 2, 5, true),
        4 => new(PowerBombs, 4, 5, false),
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    };

    /// <summary>Describes one pickup animation's instruction-loop layout and timing policy.</summary>
    /// <param name="Start">Bank-relative address of the first timed frame instruction.</param>
    /// <param name="Frames">Number of timed frame entries before the loop-control commands.</param>
    /// <param name="Duration">Menu ticks assigned to each timed frame entry.</param>
    /// <param name="HasSleep">Whether the instruction list contains a trailing sleep command after its goto pair.</param>
    internal readonly record struct PickupLoop(ushort Start, int Frames, ushort Duration, bool HasSleep)
    {
        /// <summary>Returns the mechanics word at a position in the timed-frame and loop-control portion of this program.</summary>
        /// <param name="index">Zero-based entry index, with timed frames followed by goto and loop-target words and then any trailing sleep word.</param>
        /// <returns>The instruction address and its encoded duration, pointer, or opcode value.</returns>
        internal InstructionMechanicsWord Word(int index)
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

    /// <summary>Maps a flattened index across all pickup frame entries to that frame's spritemap operand address.</summary>
    /// <param name="index">Zero-based index across the five pickup programs' presentation operands.</param>
    /// <returns>The bank-relative address of the selected frame's spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index does not identify one of the compiled presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
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

    /// <summary>Reports whether a supported enemy-death projectile kind references a word compiled from a pickup program.</summary>
    /// <param name="kind">Projectile category whose instruction address is being checked.</param>
    /// <param name="address">Bank-relative instruction word address.</param>
    /// <returns>True only for the two enemy-death categories and an address owned by a compiled pickup loop.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) =>
        (kind is RoomEnemyProjectileKind.EnemyDeathPickup or
            RoomEnemyProjectileKind.EnemyDeathExplosion) &&
        TryRead(address, out _);

    /// <summary>Reads a compiled pickup mechanics word, rejecting pointers outside the known instruction programs.</summary>
    /// <param name="address">Bank-relative word address in the pickup instruction data.</param>
    /// <returns>The compiled duration, control pointer, or opcode value at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of a compiled pickup mechanics program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Enemy-pickup mechanics pointer $86:{address:X4} is not compiled.");
    }

    /// <summary>Looks up a duration or loop-control value from the compiled pickup instruction programs.</summary>
    /// <param name="address">Bank-relative word address to search.</param>
    /// <param name="value">Receives the compiled word when the address is owned; receives zero when it is not.</param>
    /// <returns>True when the address identifies a compiled mechanics word.</returns>
    internal static bool TryRead(ushort address, out ushort value)
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

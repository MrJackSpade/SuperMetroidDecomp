namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, callback, and loop control for Yapping Maw's attack and cooldown
/// programs. Interleaved spritemap operands are compiled selectors for installed art.
/// </summary>
internal abstract class YappingMawInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUp</c> at $A8:9F6F.</summary>
    internal const ushort AttackingFacingUp = 0x9f6f;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpRight</c> at $A8:9F85.</summary>
    internal const ushort AttackingFacingUpRight = 0x9f85;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingRight</c> at $A8:9F9B.</summary>
    internal const ushort AttackingFacingRight = 0x9f9b;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownRight</c> at $A8:9FB1.</summary>
    internal const ushort AttackingFacingDownRight = 0x9fb1;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDown</c> at $A8:9FC7.</summary>
    internal const ushort AttackingFacingDown = 0x9fc7;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownLeft</c> at $A8:9FDD.</summary>
    internal const ushort AttackingFacingDownLeft = 0x9fdd;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingLeft</c> at $A8:9FF3.</summary>
    internal const ushort AttackingFacingLeft = 0x9ff3;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpLeft</c> at $A8:A009.</summary>
    internal const ushort AttackingFacingUpLeft = 0xa009;

    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpRight</c> at $A8:A01F.</summary>
    internal const ushort CooldownFacingUpRight = 0xa01f;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUp</c> at $A8:A025.</summary>
    internal const ushort CooldownFacingUp = 0xa025;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpLeft</c> at $A8:A03D.</summary>
    internal const ushort CooldownFacingUpLeft = 0xa03d;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownRight</c> at $A8:A05B.</summary>
    internal const ushort CooldownFacingDownRight = 0xa05b;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDown</c> at $A8:A061.</summary>
    internal const ushort CooldownFacingDown = 0xa061;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownLeft</c> at $A8:A079.</summary>
    internal const ushort CooldownFacingDownLeft = 0xa079;

    /// <summary><c>InstListPointers_YappingMaw</c>, adjacent selector data at $A8:A097.</summary>
    internal const ushort AdjacentAttackSelectorTable = 0xa097;

    public static int MechanicsWordCount => 96;
    public static int PresentationWordCount => 52;

    /// <summary>Yapping Maw animation holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort OpenHold = 5, ClosingHold = 3, ExtendedHold = 80, DiagonalAdjustmentHold = 4;

    /// <summary>Eight22-byte attack lists follow the native clockwise octant order.</summary>
    internal static ushort AttackForDirection(int direction)
    {
        if ((uint)direction >= 8)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Yapping Maw direction must be zero through seven.");
        return (ushort)(AttackingFacingUp + 22 * direction);
    }

    /// <summary>
    /// Each attack has four timed poses, a sound callback, and goto. Cooldown halves
    /// adjust Samus diagonally then vertically, sharing the same four-pose cooldown loop.
    /// Four-byte timed records and two-byte callbacks/control operands determine layout.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 56)
        {
            ushort start = AttackForDirection(index / 7);
            int field = index % 7;
            int offset = field < 3 ? 4 * field : field < 6 ? 4 * field - 2 : 20;
            ushort value = field switch
            {
                0 => OpenHold,
                1 or 4 => ClosingHold,
                2 => EnemyInstructionCodePointers.Instruction_YappingMaw_QueueSFXIfOnScreen,
                3 => ExtendedHold,
                5 => CommonEnemyInstructionCodes.Goto,
                _ => start,
            };
            return new((ushort)(start + offset), value);
        }
        int local = (index - 56) % 20;
        bool down = index >= 76;
        bool left = local >= 10;
        local %= 10;
        ushort entry = (ushort)((down ? CooldownFacingDownRight : CooldownFacingUpRight) + (left ? 30 : 0));
        if (local < 3)
        {
            ushort value = local switch
            {
                0 when down => left ? EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDownLeft : EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDownRight,
                0 => left ? EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUpLeft : EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUpRight,
                1 => DiagonalAdjustmentHold,
                _ => down ? EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDown : EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUp,
            };
            return new((ushort)(entry + (local == 0 ? 0 : local == 1 ? 2 : 6)), value);
        }
        return CooldownWord((ushort)(entry + 8), local - 3);
    }

    private static InstructionMechanicsWord CooldownWord(ushort entry, int field)
    {
        int offset = field < 5 ? field * 4 : 16 + 2 * (field - 4);
        ushort value = field switch
        {
            0 => ExtendedHold,
            1 or 3 => ClosingHold,
            2 => OpenHold,
            4 => EnemyInstructionCodePointers.Instruction_YappingMaw_QueueSFXIfOnScreen,
            5 => CommonEnemyInstructionCodes.Goto,
            _ => entry,
        };
        return new((ushort)(entry + offset), value);
    }

    /// <summary>Visual operands lie two bytes into timed poses; sound/carry callbacks occupy two bytes.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 32)
        {
            int pose = index % 4;
            return (ushort)(AttackForDirection(index / 4) + 2 + pose * 4 + (pose >= 2 ? 2 : 0));
        }
        int local = (index - 32) % 10;
        ushort entry = (ushort)((index < 42 ? CooldownFacingUpRight : CooldownFacingDownRight) + (local >= 5 ? 30 : 0));
        int poseInGroup = local % 5;
        return (ushort)(entry + (poseInGroup == 0 ? 4 : 10 + 4 * (poseInGroup - 1)));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0, high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var word = MechanicsWord(middle);
            if (word.Address == address) return word.Value;
            if (word.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Yapping Maw instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort word = MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}

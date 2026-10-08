namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Stoke's walking and attack programs.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal abstract class StokeInstructionProgramDefinitions
{
    /// <summary><c>InstList_Stoke_MovingLeft_0</c> at $A2:8932.</summary>
    internal const ushort MovingLeft = 0x8932;
    /// <summary><c>InstList_Stoke_AttackingLeft</c> at $A2:8948.</summary>
    internal const ushort AttackingLeft = 0x8948;
    /// <summary><c>InstList_Stoke_MovingRight_0</c> at $A2:8958.</summary>
    internal const ushort MovingRight = 0x8958;
    /// <summary><c>InstList_Stoke_AttackingRight</c> at $A2:896E.</summary>
    internal const ushort AttackingRight = 0x896e;

    /// <summary>
    /// $A2:8934/8938/893C/8940, repeated for right-facing movement: original walking holds.
    /// The longer second pose has no established mathematical or semantic timing derivation.
    /// Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.
    /// </summary>
    private static readonly ushort[] WalkingFrameDurations = [8, 16, 8, 8];
    // Attack holds of 16 are the same authored cadence (reviewed under #1165).
    public static int MechanicsWordCount => 26;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int side = index / 13;
        int local = index % 13;
        ushort move = side == 0 ? MovingLeft : MovingRight;
        if (local == 0)
            return new(move, side == 0 ? EnemyInstructionCodePointers.Instruction_Stoke_SetMovingLeft
                : EnemyInstructionCodePointers.Instruction_Stoke_SetMovingRight);
        if (local < 5)
            return new((ushort)(move + 2 + 4 * (local - 1)), WalkingFrameDurations[local - 1]);
        if (local < 7)
            return new((ushort)(move + 18 + 2 * (local - 5)), local == 5 ? CommonEnemyInstructionCodes.Goto : (ushort)(move + 2));
        ushort attack = side == 0 ? AttackingLeft : AttackingRight;
        int word = local - 7;
        int offset = word == 0 ? 0 : word < 4 ? 2 + 2 * word : 4 + 2 * word;
        ushort value = word switch
        {
            0 or 3 => 16,
            1 => EnemyInstructionCodePointers.Instruction_Stoke_SpawnFireball,
            2 => (ushort)side,
            4 => CommonEnemyInstructionCodes.Goto,
            _ => move,
        };
        return new((ushort)(attack + offset), value);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Stoke instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

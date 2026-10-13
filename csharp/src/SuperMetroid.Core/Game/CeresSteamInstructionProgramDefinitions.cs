namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the four directional Ceres steam programs.
/// Extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class CeresSteamInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_CeresSteam_Up_0</c> at $A6:F04D. Its 17 mechanics words
    /// through $F07F match the pinned NTSC J/U v1.0 ROM. The bounded program
    /// hides steam, shows its first frame for one tick, then branches back to
    /// $F04D while the activation timer remains nonzero or to $F061 when it
    /// expires. The four directional programs share this control template;
    /// presentation pointers are separate live cartridge operands.
    /// </summary>
    internal const ushort Up = 0xf04d;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_0</c> at $A6:F081, selected by both left
    /// variants 1 and 4. All 17 mechanics addresses are the upward program's
    /// addresses plus $34. The pinned ROM preserves 14 operand values exactly
    /// and relocates only its three local branch targets by $34, with zero
    /// mismatches. It therefore has the same bounded activation state graph.
    /// </summary>
    internal const ushort Left = 0xf081;
    /// <summary>
    /// <c>InstList_CeresSteam_Down_0</c> at $A6:F0B5, selected by variant 2.
    /// All 17 mechanics addresses are the upward program's addresses plus
    /// $68. Direct pinned-ROM comparison preserves 14 values and relocates
    /// only the three local branch targets by $68, with zero mismatches.
    /// The bounded activation graph is consequently the same as upward.
    /// </summary>
    internal const ushort Down = 0xf0b5;
    /// <summary>
    /// <c>InstList_CeresSteam_Right_0</c> at $A6:F0E9, selected by variants
    /// 3 and 5. All 17 mechanics addresses are the upward program's addresses
    /// plus $9C. Direct pinned-ROM comparison preserves 14 values and moves
    /// only three local branch targets by $9C, with zero mismatches. The
    /// bounded activation graph is the same as the other directions.
    /// </summary>
    internal const ushort Right = 0xf0e9;

    /// <summary>
    /// Seventeen compiled control words per directional steam program. Each program has
    /// nine separate live extended-spritemap operands: two repeat its first frame and
    /// seven active operands select consecutive ten-byte records. The interpreter reads
    /// only these reachable operands; the four directional sequences match the pinned
    /// NTSC J/U v1.0 ROM.
    /// </summary>
    public static int MechanicsWordCount => 68;

    /// <summary>
    /// Each52-byte directional program waits invisibly for activation, holds hidden
    /// for64 ticks, displays seven three-tick frames and returns to the hidden hold.
    /// Its seventeen mechanics words interleave nine presentation operands.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = (ushort)(Up + 52 * (index / 17));
        int word = index % 17;
        if (word is >= 8 and < 15)
            return new((ushort)(start + 20 + 4 * (word - 8)), 3);
        return word switch
        {
            0 => new(start, (ushort)CeresSteamInstruction.SetToIntangibleAndInvisible),
            1 => new((ushort)(start + 2), 1),
            2 => new((ushort)(start + 6), (ushort)CeresSteamInstruction.DecrementActivationTimerGotoYOrY2),
            3 => new((ushort)(start + 8), start),
            4 => new((ushort)(start + 10), (ushort)(start + 20)),
            5 => new((ushort)(start + 12), (ushort)CeresSteamInstruction.SetToIntangibleAndInvisible),
            6 => new((ushort)(start + 14), 64),
            7 => new((ushort)(start + 18), (ushort)CeresSteamInstruction.SetToTangibleAndVisible),
            15 => new((ushort)(start + 48), (ushort)CommonEnemyInstruction.Goto),
            _ => new((ushort)(start + 50), (ushort)(start + 12)),
        };
    }
    /// <summary>Finds a compiled bank-$A6 control word by address and rejects presentation-operand addresses.</summary>
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
            $"Ceres steam instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }
}

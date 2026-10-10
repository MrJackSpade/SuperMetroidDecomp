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
    /// Addresses of live extended-spritemap operands, separate from compiled
    /// mechanics. For the upward program, the operands at $F051 and $F05D
    /// reuse $F142. Its seven active operands at <c>$F063 + 4 * frame</c>,
    /// for frames 0..6, contain <c>$F142 + $0A * frame</c>. All nine words
    /// match the pinned NTSC J/U v1.0 ROM. The stride is the size of each
    /// extended-spritemap record; the authored payloads remain cartridge data.
    /// For the leftward program, $F085 and $F091 repeat $F188; its active
    /// operands at <c>$F097 + 4 * frame</c> contain
    /// <c>$F188 + $0A * frame</c> for frames 0..6. All nine pinned-ROM words
    /// match, and the interpreter reads only these reachable frame operands.
    /// For the downward program, $F0B9 and $F0C5 repeat $F1CE; the seven
    /// active operands at <c>$F0CB + 4 * frame</c> contain
    /// <c>$F1CE + $0A * frame</c> for frames 0..6. All nine pinned-ROM words
    /// match the bounded seven-record sequence.
    /// For the rightward program, $F0ED and $F0F9 repeat $F214; the seven
    /// active operands at <c>$F0FF + 4 * frame</c> contain
    /// <c>$F214 + $0A * frame</c> for frames 0..6. All nine pinned-ROM words
    /// match, completing the four directional presentation sequences.
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
            0 => new(start, CeresEnemyCodePointers.HideCeresSteam),
            1 => new((ushort)(start + 2), 1),
            2 => new((ushort)(start + 6), CeresEnemyCodePointers.StepCeresSteamActivationTimer),
            3 => new((ushort)(start + 8), start),
            4 => new((ushort)(start + 10), (ushort)(start + 20)),
            5 => new((ushort)(start + 12), CeresEnemyCodePointers.HideCeresSteam),
            6 => new((ushort)(start + 14), 64),
            7 => new((ushort)(start + 18), EnemyInstructionCodePointers.Instruction_CeresSteam_SetToTangibleAndVisible),
            15 => new((ushort)(start + 48), CommonEnemyInstructionCodes.Goto),
            _ => new((ushort)(start + 50), (ushort)(start + 12)),
        };
    }
    /// <summary>Resolves a compiled mechanics-word address from the four directional Ceres steam programs.</summary>
    /// <param name="address">Bank-$A6 byte address of an instruction mechanics word.</param>
    /// <returns>The compiled command, timer, or branch operand at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word.</exception>
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

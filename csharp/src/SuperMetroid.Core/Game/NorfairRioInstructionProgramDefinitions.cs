namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, private callbacks, and loop control for Norfair Rio parent and flame
/// programs. Interleaved spritemap operands select extracted presentation frames.
/// </summary>
internal abstract class NorfairRioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Geruta_Main_Idle</c> at $A2:C0F1.</summary>
    internal const ushort Idle = 0xc0f1;
    /// <summary><c>InstList_Geruta_Main_Swoop_StartDescending</c> at $A2:C107.</summary>
    internal const ushort StartDescending = 0xc107;
    /// <summary><c>InstList_Geruta_Main_Swoop_Descending</c> at $A2:C12F.</summary>
    internal const ushort Descending = 0xc12f;
    /// <summary><c>InstList_Geruta_Main_Swoop_StartAscending</c> at $A2:C145.</summary>
    internal const ushort StartAscending = 0xc145;
    /// <summary><c>InstList_Geruta_Main_Swoop_Ascending</c> at $A2:C179.</summary>
    internal const ushort Ascending = 0xc179;
    /// <summary><c>InstList_Geruta_Flames_Ascending</c> at $A2:C18F.</summary>
    internal const ushort FlamesAscending = 0xc18f;
    /// <summary><c>InstList_Geruta_Flames_Descending</c> at $A2:C1A3.</summary>
    internal const ushort FlamesDescending = 0xc1a3;

    // Pose holds are authored animation cadence (reviewed under #1165); program geometry and the
    // callback dispatch below are calculated.
    /// <summary>Alternating hold durations for the two poses in Rio's idle loop.</summary>
    private static readonly ushort[] IdleHolds = [13, 18];

    /// <summary>Hold durations for the four flight poses used by Rio and its flame loops.</summary>
    private static readonly ushort[] FlightHolds = [6, 5, 8, 6];

    /// <summary>Number of compiled instruction words across Rio's parent and flame programs.</summary>
    public static int MechanicsWordCount => 65;

    /// <summary>Number of interleaved spritemap operand words selected for extracted presentation frames.</summary>
    public static int PresentationWordCount => 34;

    /// <summary>
    /// $A2:C0F1-C1B6: three parent loops, two callback/pose transition sequences,
    /// and two flame loops. Each timed pose occupies four bytes; interleaved
    /// follower-offset callbacks make the transition poses six bytes wide.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 7) return LoopWord(index, Idle, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate, true);
        if (index < 21) return TransitionWord(index - 7, StartDescending, false);
        if (index < 28) return LoopWord(index - 21, Descending, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC, false);
        if (index < 46) return TransitionWord(index - 28, StartAscending, true);
        if (index < 53) return LoopWord(index - 46, Ascending, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C_duplicate, false);
        if (index < 59) return LoopWord(index - 53, FlamesAscending, 0, false);
        return LoopWord(index - 59, FlamesDescending, 0, false);
    }

    /// <summary>Calculates one mechanics word in a looping program, including its setup, timed poses, and loop control.</summary>
    /// <param name="index">Zero-based word position within the selected loop's compiled sequence.</param>
    /// <param name="start">Instruction address at which the loop begins.</param>
    /// <param name="setup">Optional leading setup callback word, or zero when the loop has none.</param>
    /// <param name="idle"><see langword="true"/> to use the alternating idle holds; otherwise use flight-pose holds.</param>
    /// <returns>The address and value compiled for that mechanics word.</returns>
    private static InstructionMechanicsWord LoopWord(int index, ushort start, ushort setup, bool idle)
    {
        int frameStart = start;
        if (setup != 0)
        {
            if (index == 0) return new(start, setup);
            index--;
            frameStart += 2;
        }
        if (index < 4) return new((ushort)(frameStart + index * 4), idle ? IdleHolds[index % 2] : FlightHolds[index]);
        return new((ushort)(frameStart + 16 + (index - 4) * 2), index == 4 ? CommonEnemyInstructionCodes.Goto : start);
    }

    /// <summary>Calculates a transition word with pose callbacks followed by its completion or sleep command.</summary>
    /// <param name="index">Zero-based word position in the selected transition sequence.</param>
    /// <param name="start">Instruction address where the transition sequence begins.</param>
    /// <param name="ascending"><see langword="true"/> for the longer ascending transition and its callback order.</param>
    /// <returns>The address and value compiled for that transition word.</returns>
    private static InstructionMechanicsWord TransitionWord(int index, ushort start, bool ascending)
    {
        int poses = ascending ? 8 : 6;
        if (index < poses * 2)
            return new((ushort)(start + index / 2 * 6 + index % 2 * 2),
                index % 2 == 0 ? TransitionCallback(index / 2, ascending) : (ushort)1);
        return new((ushort)(start + poses * 6 + (index - poses * 2) * 2),
            index == poses * 2 ? NorfairRioInstructionCodes.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>Named per-pose dispatch: each pose selects the handler that sets the flame Y offset
    /// (-16,-12,-4,0,4,8,8,12 ascending) attaching the flame to that drawn pose. A semantic case mapping.</summary>
    private static ushort TransitionCallback(int pose, bool ascending) => ascending ? pose switch
    {
        0 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
        1 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
        2 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
        3 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
        4 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
        5 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
        6 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate,
        _ => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C,
    } : pose switch
    {
        0 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
        1 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
        2 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
        3 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
        4 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
        _ => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
    };

    /// <summary>Maps a presentation-word index to its interleaved spritemap operand address.</summary>
    /// <param name="index">Zero-based index among the compiled presentation operands.</param>
    /// <returns>The instruction address containing that spritemap pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 4) return (ushort)(Idle + 4 + index * 4);
        if (index < 10) return (ushort)(StartDescending + 4 + (index - 4) * 6);
        if (index < 14) return (ushort)(Descending + 4 + (index - 10) * 4);
        if (index < 22) return (ushort)(StartAscending + 4 + (index - 14) * 6);
        if (index < 26) return (ushort)(Ascending + 4 + (index - 22) * 4);
        if (index < 30) return (ushort)(FlamesAscending + 2 + (index - 26) * 4);
        return (ushort)(FlamesDescending + 2 + (index - 30) * 4);
    }

    /// <summary>Determines whether an address contains a spritemap operand rather than mechanics data.</summary>
    /// <param name="address">Instruction address to classify.</param>
    /// <returns><see langword="true"/> when the address is one of the compiled presentation operands.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }

    /// <summary>Looks up the compiled instruction value at a mechanics-word address.</summary>
    /// <param name="address">Bank-$A2 address of the mechanics word to resolve.</param>
    /// <returns>The compiled command or timing value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of either compiled Rio program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

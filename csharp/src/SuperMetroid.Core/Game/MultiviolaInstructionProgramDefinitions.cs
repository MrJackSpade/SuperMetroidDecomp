namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Multiviola's production animation loop. The
/// interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class MultiviolaInstructionProgramDefinitions
{
    /// <summary><c>InstList_Multiviola</c> at $A2:B2DC.</summary>
    internal const ushort Flying = 0xb2dc;

    /// <summary>$A2:B2DC-B310: fourteen timed poses (0..7 then 6..1), ten ticks each.</summary>
    private const int TimedFrameCount = 14;
    /// <summary>$A2:B2DC-B310 InstList_Multiviola uses one shared ten-tick frame cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort FrameDuration = 10;
    /// <summary>$A2:B314 Instruction_Common_GotoY after the timed loop.</summary>
    private const ushort LoopOpcode = Flying + TimedFrameCount * 4;

    /// <summary>Number of compiled timing and loop-control words in the animation program.</summary>
    public static int MechanicsWordCount => TimedFrameCount + 2;

    /// <summary>Number of interleaved spritemap selector words for the fourteen timed poses.</summary>
    public static int PresentationWordCount => TimedFrameCount;

    /// <summary>Gets a timed-pose or loop-control word by its mechanics slot.</summary>
    /// <param name="index">The zero-based slot in the compiled mechanics-word sequence.</param>
    /// <returns>The instruction address and fixed timer or loop operand stored in that slot.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return index < TimedFrameCount ? new((ushort)(Flying + index * 4), FrameDuration)
            : new((ushort)(LoopOpcode + (index - TimedFrameCount) * 2),
                index == TimedFrameCount ? CommonEnemyInstructionCodes.Goto : Flying);
    }
    /// <summary>Gets the address of one pose's interleaved spritemap selector.</summary>
    /// <param name="index">The zero-based timed-pose index, from zero through thirteen.</param>
    /// <returns>The bank-$A2 address containing that pose's presentation word.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= TimedFrameCount) throw new IndexOutOfRangeException();
        return (ushort)(Flying + index * 4 + 2);
    }
    /// <summary>Looks up a compiled timing or loop-control operand by instruction address.</summary>
    /// <param name="address">The bank-$A2 address of a mechanics word.</param>
    /// <returns>The fixed word encoded at that instruction address.</returns>
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
            $"Multiviola instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

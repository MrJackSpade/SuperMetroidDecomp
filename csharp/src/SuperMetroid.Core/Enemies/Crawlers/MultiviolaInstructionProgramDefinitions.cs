using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

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

    /// <summary>Fourteen pose durations plus a jump opcode and its target.</summary>
    public static int MechanicsWordCount => TimedFrameCount + 2;
    /// <summary>Fourteen interleaved spritemap operands trace the out-and-back pose loop.</summary>
    public static int PresentationWordCount => TimedFrameCount;
    /// <summary>Compiles ten-tick poses followed by a jump back to the first pose.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return index < TimedFrameCount ? new((ushort)(Flying + index * 4), FrameDuration)
            : new((ushort)(LoopOpcode + (index - TimedFrameCount) * 2),
                index == TimedFrameCount ? (ushort)CommonEnemyInstruction.Goto : Flying);
    }
    /// <summary>Locates the spritemap operand two bytes after each pose duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= TimedFrameCount) throw new IndexOutOfRangeException();
        return (ushort)(Flying + index * 4 + 2);
    }
    /// <summary>Reads only compiled Multiviola control words from the bank-$A2 loop.</summary>
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

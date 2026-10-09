namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing and loop control for Kraid's two reusable fingernail actors.
/// The eight interleaved spritemap operands select compiled presentation identities.
/// </summary>
internal abstract class KraidNailInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidNail</c> at $A7:8B0A.</summary>
    internal const ushort Loop = 0x8b0a;

    /// <summary>Number of spritemap operands in the nail's eight timed animation frames.</summary>
    public static int PresentationWordCount => 8;

    /// <summary>Number of compiled frame-duration and loop-control words, including the eight frame entries.</summary>
    public static int MechanicsWordCount => PresentationWordCount + 2;

    /// <summary>Eight four-byte frames last three ticks each, followed by Goto
    /// and its loop target. Only exact duration/control starts are mechanics words.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return index < PresentationWordCount
            ? new((ushort)(Loop + 4 * index), 3)
            : new((ushort)(Loop + 4 * PresentationWordCount + 2 * (index - PresentationWordCount)),
                index == PresentationWordCount ? CommonEnemyInstructionCodes.Goto : Loop);
    }

    /// <summary>Determines whether an instruction-list address points to a frame's spritemap operand.</summary>
    /// <param name="address">Bank-$A7 address to classify within the nail instruction loop.</param>
    /// <returns><see langword="true"/> for one of the eight presentation operands.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 4 * PresentationWordCount && offset % 4 == 0;
    }

    /// <summary>Resolves a frame duration, loop opcode, or loop target from the compiled nail instruction list.</summary>
    /// <param name="address">Bank-$A7 address of the mechanics word to read.</param>
    /// <returns>The word stored at the requested mechanics address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Kraid fingernail mechanics pointer $A7:{address:X4} is not compiled.");
    }
}

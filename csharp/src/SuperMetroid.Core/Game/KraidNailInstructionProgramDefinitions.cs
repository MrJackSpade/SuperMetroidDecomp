namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing and loop control for Kraid's two reusable fingernail actors.
/// The eight interleaved spritemap operands select compiled presentation identities.
/// </summary>
internal abstract class KraidNailInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidNail</c> at $A7:8B0A.</summary>
    internal const ushort Loop = 0x8b0a;

    public static int PresentationWordCount => 8;
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
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 4 * PresentationWordCount && offset % 4 == 0;
    }

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

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing and loop control for Kraid's two reusable fingernail actors.
/// The eight interleaved spritemap operands select compiled presentation identities.
/// </summary>
internal abstract class KraidNailInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidNail</c> at $A7:8B0A.</summary>
    internal const ushort Loop = 0x8b0a;

    /// <summary>Eight native fingernail poses whose spritemap operands are resolved separately.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Eight three-tick waits followed by the goto opcode and its loop pointer.</summary>
    public static int MechanicsWordCount => PresentationWordCount + 2;

    /// <summary>Eight four-byte frames last three ticks each, followed by Goto
    /// and its loop target. Only exact duration/control starts are mechanics words.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return index < PresentationWordCount
            ? new((ushort)(Loop + 4 * index), 3)
            : new((ushort)(Loop + 4 * PresentationWordCount + 2 * (index - PresentationWordCount)),
                index == PresentationWordCount ? (ushort)CommonEnemyInstruction.Goto : Loop);
    }
    /// <summary>Identifies a spritemap operand two bytes after any of the eight timed frames.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 4 * PresentationWordCount && offset % 4 == 0;
    }

    /// <summary>Looks up a compiled bank-$A7 wait or loop word and rejects visual operands and other addresses.</summary>
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

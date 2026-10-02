namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid fingernail mechanics word at its bank-$A7 address.</summary>
internal readonly record struct KraidNailInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and loop control for Kraid's two reusable fingernail actors.
/// The eight interleaved spritemap operands select compiled presentation identities.
/// </summary>
internal static class KraidNailInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidNail</c> at $A7:8B0A.</summary>
    internal const ushort Loop = 0x8b0a;
    /// <summary>First adjacent unused extended-spritemap record at $A7:8B2E.</summary>
    internal const ushort AdjacentPresentationData = 0x8b2e;

    internal const int PresentationWordCount = 8;
    internal const int MechanicsWordCount = PresentationWordCount + 2;

    /// <summary>Eight four-byte frames last three ticks each, followed by Goto
    /// and its loop target. Only exact duration/control starts are mechanics words.</summary>
    internal static KraidNailInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return index < PresentationWordCount
            ? new((ushort)(Loop + 4 * index), 3)
            : new((ushort)(Loop + 4 * PresentationWordCount + 2 * (index - PresentationWordCount)),
                index == PresentationWordCount ? CommonEnemyInstructionCodes.Goto : Loop);
    }

    /// <summary>Each frame's visual operand is two bytes after its duration.</summary>
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Loop + 4 * index + 2);
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

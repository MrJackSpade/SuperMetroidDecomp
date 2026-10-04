namespace SuperMetroid.Core.Game;

internal readonly record struct FlyInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for the shared Mellow, Mella, and Memu animation loop.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal static class FlyInstructionProgramDefinitions
{
    /// <summary><c>InstList_Mellow_Mella_Menu</c> at $A2:B013.</summary>
    internal const ushort Flight = 0xb013;

    /// <summary>Four two-frame drawings followed by goto-first-frame.</summary>
    private const int FrameCount = 4;
    internal static int MechanicsWordCount => FrameCount + 2;
    internal static int PresentationWordCount => FrameCount;

    internal static FlyInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < FrameCount) return new((ushort)(Flight + 4 * index), 2);
        return new((ushort)(Flight + 4 * FrameCount + 2 * (index - FrameCount)),
            index == FrameCount ? CommonEnemyInstructionCodes.Goto : Flight);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Flight + 2 + 4 * index);
    }

    /// <summary>True only for the visual operand in each of the four drawing records.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Flight + 2);
        return (uint)offset < 4 * FrameCount && offset % 4 == 0;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Fly-family instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
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

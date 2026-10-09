namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the shared Mellow, Mella, and Memu animation loop.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class FlyInstructionProgramDefinitions
{
    /// <summary><c>InstList_Mellow_Mella_Menu</c> at $A2:B013.</summary>
    internal const ushort Flight = 0xb013;

    /// <summary>Four two-frame drawings followed by goto-first-frame.</summary>
    internal const int FrameCount = 4;

    /// <summary>Gets the number of control and timing words in the compiled flight loop, excluding visual operands.</summary>
    public static int MechanicsWordCount => FrameCount + 2;

    /// <summary>Describes one indexed control or timing word in the four-frame flight loop.</summary>
    /// <param name="index">The mechanics-word index, including the final goto and loop target.</param>
    /// <returns>The word's native address and value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < FrameCount) return new((ushort)(Flight + 4 * index), 2);
        return new((ushort)(Flight + 4 * FrameCount + 2 * (index - FrameCount)),
            index == FrameCount ? CommonEnemyInstructionCodes.Goto : Flight);
    }

    /// <summary>True only for the visual operand in each of the four drawing records.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Flight + 2);
        return (uint)offset < 4 * FrameCount && offset % 4 == 0;
    }

    /// <summary>Resolves an address in the compiled flight loop to its control or timing word.</summary>
    /// <param name="address">The bank-$A2 instruction address to look up.</param>
    /// <returns>The mechanics word stored at the address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the compiled mechanics-word locations.</exception>
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
}

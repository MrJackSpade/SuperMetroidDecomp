namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Sbug's eight directional animation loops.</summary>
/// <remarks>
/// Each list's frame durations and terminal goto are immutable simulation control. The
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal abstract class SbugInstructionProgramDefinitions
{
    /// <summary><c>$A3:A071</c>, right-facing animation loop.</summary>
    internal const ushort Right = 0xa071;

    /// <summary>Gets the 48 duration and branch words across eight four-frame animation loops.</summary>
    public static int MechanicsWordCount => 48;

    /// <summary>Gets the four interleaved spritemap operands exposed by each of the eight directional loops.</summary>
    public static int PresentationWordCount => 32;

    /// <summary>Eight directional programs each display four five-tick frames and branch back to their entry.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = (ushort)(Right + 20 * (index / 6));
        int word = index % 6;
        return word < 4 ? new((ushort)(start + 4 * word), 5)
            : new((ushort)(start + 16 + 2 * (word - 4)), word == 4 ? CommonEnemyInstructionCodes.Goto : start);
    }

    /// <summary>Returns the bank-local address of one live spritemap operand in the directional animation lists.</summary>
    /// <param name="index">Zero-based presentation operand position across all eight loops.</param>
    /// <returns>The address of the selected interleaved operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Right + 20 * (index / 4) + 2 + 4 * (index % 4));
    }
    /// <summary>Returns one fixed control word or rejects pointers outside all eight loops.</summary>
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
            $"Sbug instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}

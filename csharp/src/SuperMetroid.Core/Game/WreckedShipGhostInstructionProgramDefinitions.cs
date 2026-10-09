namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Wrecked Ship ghost (native Coven) animation
/// loop. Its three spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class WreckedShipGhostInstructionProgramDefinitions
{
    /// <summary><c>InstList_Coven</c> at $A8:9A8C.</summary>
    internal const ushort Floating = 0x9a8c;

    /// <summary>The terminal <c>Instruction_Common_GotoY</c> word at $A8:9A98.</summary>
    internal const ushort LoopOpcode = 0x9a98;

    /// <summary>Number of timing and loop-control words in the compiled ghost animation.</summary>
    public static int MechanicsWordCount => 5;
    /// <summary>Number of sprite-selection operands interleaved with the ghost's mechanics words.</summary>
    public static int PresentationWordCount => 3;

    /// <summary>Three sixteen-tick poses followed by the native goto and loop target.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < PresentationWordCount) return new((ushort)(Floating + 4 * index), 16);
        return index == PresentationWordCount
            ? new(LoopOpcode, CommonEnemyInstructionCodes.Goto)
            : new((ushort)(LoopOpcode + 2), Floating);
    }

    /// <summary>Maps a compiled pose index to its interleaved bank-$A8 sprite-selection operand.</summary>
    /// <param name="index">Zero-based index of one of the three displayed poses.</param>
    /// <returns>Address of that pose's presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the three compiled poses.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Floating + 2 + 4 * index);
    }
    /// <summary>Reads a timing or loop-control word from the compiled ghost instruction list.</summary>
    /// <param name="address">Bank-$A8 address of a mechanics word.</param>
    /// <returns>The hold duration, goto opcode, or loop target at the address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Wrecked Ship ghost instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
}

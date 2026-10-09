namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled one-pose sleep program for Polyp; its visual selector uses installed artwork.
/// </summary>
internal abstract class PolypInstructionProgramDefinitions
{
    /// <summary><c>InstList_Polyp</c> at $A2:B51A.</summary>
    internal const ushort Stationary = 0xb51a;

    /// <summary>The compiled <c>Spritemap_Polyp</c> operand at $A2:B51C.</summary>
    internal const ushort PresentationWord = 0xb51c;

    /// <summary><c>Spritemap_Polyp</c> at $A2:B5FB, selected by the one stationary pose at $B51C.</summary>
    internal const ushort StationaryFrame = 0xb5fb;

    /// <summary>Resolves Polyp's sole presentation operand to its stationary spritemap pointer.</summary>
    /// <param name="operandAddress">Address of the compiled spritemap operand.</param>
    /// <returns>The stationary frame pointer selected by that operand.</returns>
    /// <exception cref="InvalidDataException">The address is not Polyp's presentation operand.</exception>
    internal static ushort FrameAt(ushort operandAddress) =>
        operandAddress == PresentationWord ? StationaryFrame
            : throw new InvalidDataException("Polyp visual operand is outside its stationary program.");

    /// <summary>Gets the two mechanics words: a one-tick duration followed by the shared sleep command.</summary>
    public static int MechanicsWordCount => 2;

    /// <summary>Returns the duration or sleep command at a zero-based position in Polyp's stationary program.</summary>
    /// <param name="index">Mechanics-word index: zero selects the duration and one selects the sleep command.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the two-word program.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return new((ushort)(Stationary + index * 4),
            index == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>Resolves an exact compiled mechanics-word address in Polyp's stationary program.</summary>
    /// <param name="address">Bank-$A2 address to look up.</param>
    /// <returns>The mechanics word stored at the matching address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify either compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }
        throw new InvalidDataException(
            $"Polyp instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the dead Zoomer, Ripper, and Skree corpse
/// programs. Their eight spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class DeadTourianCorpseInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseZoomer_Param1_0</c> at $A9:ECF5.</summary>
    internal const ushort Zoomer0 = 0xecf5;

    /// <summary>Number of corpse instruction lists, one each for the three enemy types and their parameter variants.</summary>
    internal static int ProgramCount => 8;

    /// <summary>Number of compiled duration and sleep words across all corpse lists; sprite operands are presentation-owned.</summary>
    public static int MechanicsWordCount => 16;

    /// <summary>Returns the bank-local start address of one six-byte corpse instruction list.</summary>
    /// <param name="index">Zero-based list index from zero through <see cref="ProgramCount"/> minus one.</param>
    /// <returns>The address of the list's one-tick duration word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the eight compiled lists.</exception>
    internal static ushort Program(int index) => (uint)index < ProgramCount
        ? (ushort)(Zoomer0+6*index) : throw new IndexOutOfRangeException();

    /// <summary>Maps a flattened mechanics-word index to a list's duration or terminal sleep command.</summary>
    /// <param name="index">Zero-based position in the sixteen-word mechanics sequence.</param>
    /// <returns>The bank-local address and value of the selected duration or sleep word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = Program(index/2);
        return (index&1) == 0 ? new(start,1) : new((ushort)(start+4),CommonEnemyInstructionCodes.Sleep);
    }
    /// <summary>Reads a compiled control word while leaving the separately installed spritemap operands unresolved.</summary>
    /// <param name="address">Bank-local address of a corpse-list duration or sleep word.</param>
    /// <returns>The compiled value stored at the requested mechanics address.</returns>
    /// <exception cref="InvalidDataException"><paramref name="address"/> is not one of the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Dead Tourian corpse instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }
}

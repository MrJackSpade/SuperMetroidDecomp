using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BotwoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BotwoonInstructionProgramDefinitions))]
internal abstract class BotwoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap operands in the compiled Botwoon presentation program.</summary>
    public static int PresentationWordCount => BotwoonInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a presentation-table position to the native spritemap operand address.</summary>
    /// <param name="index">Zero-based position in the compiled presentation table.</param>
    /// <returns>The bank-$B3 address containing the selected operand.</returns>
    public static ushort PresentationWordAddress(int index) => BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled mechanics words across movement, hidden, and directional spit instruction lists.</summary>
    public static int MechanicsWordCount => 74;

    /// <summary>Returns the address and value of a mechanics word in the catalog's stable enumeration order.</summary>
    /// <param name="index">Zero-based position in the combined mechanics table.</param>
    /// <returns>The bank-$B3 instruction address and its native operand value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address;
        if (index < 24)
        {
            int word = index % 3;
            address = (ushort)(BotwoonInstructionProgramDefinitions.MovingUpLeft + 8 * BotwoonInstructionProgramDefinitions.PhysicalDirection(index / 3) + (word == 0 ? 0 : word == 1 ? 2 : 6));
        }
        else if (index < 26) address = (ushort)(BotwoonInstructionProgramDefinitions.Hidden + 4 * (index - 24));
        else
        {
            int word = (index - 26) % 6;
            int offset = word == 0 ? 0 : word == 5 ? 14 : 2 + 2 * word;
            address = (ushort)(BotwoonInstructionProgramDefinitions.SpittingUpLeft + 16 * BotwoonInstructionProgramDefinitions.PhysicalDirection((index - 26) / 6) + offset);
        }
        return new(address, BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Determines whether an address names a fixed mechanics word in the compiled Botwoon instruction lists.</summary>
    /// <param name="address">Low sixteen bits of the candidate bank-$B3 address.</param>
    /// <returns><see langword="true"/> for a recognized fixed word, including the two hidden-list operands.</returns>
    internal static bool IsMechanicsWord(ushort address) => address == BotwoonInstructionProgramDefinitions.Hidden || address == BotwoonInstructionProgramDefinitions.Hidden + 4 ||
        (BotwoonInstructionProgramDefinitions.TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 0 or 4 or 6 or 8 or 10 or 14 : offset is 0 or 2 or 6));
    /// <summary>Checks whether a bank-$B3 byte address overlaps either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full SNES address to inspect.</param>
    /// <returns><see langword="true"/> when the address is in bank $B3 and belongs to a compiled word.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (IsMechanicsWord(unchecked((ushort)address)) || IsMechanicsWord(unchecked((ushort)(address - 1))));
}

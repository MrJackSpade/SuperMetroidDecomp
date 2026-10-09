using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="LowerNorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(LowerNorfairRioInstructionProgramDefinitions))]
internal abstract class LowerNorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words across the Lower Norfair Rio programs.</summary>
    public static int MechanicsWordCount => 51;

    /// <summary>Number of interleaved spritemap operands supplied by Rio presentation artwork.</summary>
    public static int PresentationWordCount => 32;

    /// <summary>Returns a mechanics word from the contiguous compiled program range in address order.</summary>
    /// <param name="index">Zero-based index among mechanics words, excluding presentation operands.</param>
    /// <returns>The bank-$A2 address and value of the selected control, timing, or callback word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the 51 compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = LowerNorfairRioInstructionProgramDefinitions.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (!LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return new((ushort)address, LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Returns the native address of one interleaved Rio spritemap operand.</summary>
    /// <param name="index">Zero-based index among the presentation words.</param>
    /// <returns>The bank-$A2 address whose word is supplied by presentation data.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the 32 presentation slots.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int address = LowerNorfairRioInstructionProgramDefinitions.Idle; address < LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
        {
            if (LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord((ushort)address) && index-- == 0)
                return (ushort)address;
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Checks whether a byte in the Rio program range belongs to a compiled mechanics word.</summary>
    /// <param name="address">Full 24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a bank-$A2 mechanics word, excluding presentation operands.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort word = (ushort)(address & ~1);
        return LowerNorfairRioInstructionProgramDefinitions.ProgramAt(word) != 0 && !LowerNorfairRioInstructionProgramDefinitions.IsPresentationWord(word);
    }
}

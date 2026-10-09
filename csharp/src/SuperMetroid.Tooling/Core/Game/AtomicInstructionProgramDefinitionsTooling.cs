using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AtomicInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AtomicInstructionProgramDefinitions))]
internal abstract class AtomicInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    // Four loops: six duration/visual pairs followed by goto and its target.
    /// <summary>Number of mechanics words across the four compiled animation loops, including each loop's goto and target.</summary>
    public static int MechanicsWordCount => 4 * 8;
    /// <summary>Number of visual operand words across the four compiled animation loops.</summary>
    public static int PresentationWordCount => 4 * 6;

    /// <summary>Gets the mechanics word and its address at the specified position in the compiled loop data.</summary>
    /// <param name="index">Zero-based mechanics-word index, ordered by address across all four loops.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        ushort address = (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 8) +
            (word < 6 ? 4 * word : 24 + 2 * (word - 6)));
        return new(address, AtomicInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Gets the ROM address of a visual operand in the compiled loop data.</summary>
    /// <param name="index">Zero-based visual-operand index, ordered by address across all four loops.</param>
    /// <returns>The bank-local address of the selected visual operand word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(AtomicInstructionProgramDefinitions.UpRight + 28 * (index / 6) + 4 * (index % 6) + 2);
    }

    /// <summary>Checks whether an address identifies a mechanics byte in one of the four compiled loops.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics byte in bank $A8; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = (ushort)address - AtomicInstructionProgramDefinitions.UpRight;
        if ((uint)offset >= 4 * 28) return false;
        int stage = offset % 28;
        return stage >= 24 || stage % 4 < 2;
    }
}

using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidNailInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidNailInstructionProgramDefinitions))]
internal abstract class KraidNailInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of frame-duration and loop-control words exposed by the compiled nail instruction list.</summary>
    public static int MechanicsWordCount => KraidNailInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets the address and value of one frame, loop opcode, or loop target mechanics word.</summary>
    /// <param name="index">Zero-based mechanics-word index from zero through <see cref="MechanicsWordCount"/> minus one.</param>
    /// <returns>The bank-$A7 address and compiled value at that index.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidNailInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of timed-frame spritemap operands available for extraction.</summary>
    public static int PresentationWordCount => KraidNailInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Each frame's visual operand is two bytes after its duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= KraidNailInstructionProgramDefinitions.PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KraidNailInstructionProgramDefinitions.Loop + 4 * index + 2);
    }
    /// <summary>Checks whether a full banked address identifies either byte of a compiled mechanics word.</summary>
    /// <param name="address">Banked address to classify.</param>
    /// <returns><see langword="true"/> for a byte in the bank-$A7 timing or loop-control words; otherwise <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidNailInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidNailInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

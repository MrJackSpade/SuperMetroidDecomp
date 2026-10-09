using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="OwtchInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(OwtchInstructionProgramDefinitions))]
internal abstract class OwtchInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of callback, pose-duration, and loop words across Owtch's two direction programs.</summary>
    public static int MechanicsWordCount => 12;

    /// <summary>Number of interleaved shell-pose spritemap operands supplied by the visual catalog.</summary>
    public static int PresentationWordCount => 6;

    /// <summary>Resolves a mechanics ordinal to the native instruction address and compiled word value.</summary>
    /// <param name="index">Zero-based index across the left- and right-moving programs.</param>
    /// <returns>The bank-local address and control value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 6;
        int offset = word == 0 ? 0 : word <= 4 ? 2 + (word - 1) * 4 : 16;
        ushort address = (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 6 * OwtchInstructionProgramDefinitions.ProgramBytes + offset);
        return new(address, OwtchInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the native address of a shell-pose spritemap operand by ordinal.</summary>
    /// <param name="index">Zero-based index among the six presentation operands.</param>
    /// <returns>The bank-local address supplied by the installed visual catalog.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 3 * OwtchInstructionProgramDefinitions.ProgramBytes + 4 + index % 3 * 4);
    }
    /// <summary>Checks whether a full address refers to either byte of a compiled Owtch mechanics word.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A2; presentation operands and unrelated addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - OwtchInstructionProgramDefinitions.MovingLeft;
        return (uint)offset < 2 * OwtchInstructionProgramDefinitions.ProgramBytes &&
            ((offset % OwtchInstructionProgramDefinitions.ProgramBytes & ~1) is 0 or 2 or 6 or 10 or 14 or 16);
    }
}

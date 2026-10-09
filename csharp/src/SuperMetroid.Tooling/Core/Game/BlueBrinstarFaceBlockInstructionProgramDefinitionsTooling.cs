using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BlueBrinstarFaceBlockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BlueBrinstarFaceBlockInstructionProgramDefinitions))]
internal abstract class BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timing and control words across both approach animations and the initial program.</summary>
    public static int MechanicsWordCount => 10;

    /// <summary>Number of spritemap operands referenced by the approach and initial programs.</summary>
    public static int PresentationWordCount => 7;

    /// <summary>Resolves a mechanics slot to its native address and compiled timer or control value.</summary>
    /// <param name="index">Zero-based index among the ten compiled mechanics words.</param>
    /// <returns>The bank-local address and value for the selected slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 8 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 4) + 4 * (index % 4))
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 4 * (index - 8));
        return new(address, BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the native address of an extracted spritemap operand by slot index.</summary>
    /// <param name="index">Zero-based index among the seven presentation operands.</param>
    /// <returns>The bank-local address of the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < 6 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 3) + 4 * (index % 3) + 2)
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 2);
    }
    /// <summary>Checks whether a full SNES address points to a byte of a compiled timer or control word.</summary>
    /// <param name="address">Full 24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for mechanics bytes in bank $A8; presentation operands and other addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft;
        if ((uint)offset < 28) return offset % 14 % 4 < 2;
        int initialOffset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial;
        return (uint)initialOffset < 6 && initialOffset % 4 < 2;
    }
}

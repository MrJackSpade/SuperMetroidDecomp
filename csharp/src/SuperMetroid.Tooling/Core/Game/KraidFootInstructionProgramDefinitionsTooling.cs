using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidFootInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidFootInstructionProgramDefinitions))]
internal abstract class KraidFootInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Two initial frames, two 36-frame forward programs and 32 backward frames.</summary>
    public static int PresentationWordCount => 2 + 2 * 36 + 32;
    /// <summary>Every other word in the bounded native program region is mechanics.</summary>
    public static int MechanicsWordCount => (KraidFootInstructionProgramDefinitions.AdjacentUnusedFastBackward - KraidFootInstructionProgramDefinitions.Initial) / 2 - PresentationWordCount;
    /// <summary>Generates the indexed mechanics word from Kraid foot's native instruction-program grammar.</summary>
    /// <param name="index">Zero-based position among the compiled mechanics words.</param>
    /// <returns>The address and value of the selected instruction, timing, callback, or flow-control word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(index, -1, false);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(-1);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return words.Selected;
    }
    /// <summary>Generates the address of one frame selector embedded in Kraid foot's native programs.</summary>
    /// <param name="index">Zero-based position among the 106 presentation operands.</param>
    /// <returns>The address of the selected frame's presentation operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation-operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(-1, -1, false);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(index);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return presentation.Selected;
    }
    /// <summary>Checks whether a full address names either byte of a generated mechanics word in bank $A7.</summary>
    /// <param name="address">Full banked address to classify.</param>
    /// <returns><see langword="true"/> for compiled mechanics bytes; presentation operands and addresses in other banks return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000) return false;
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(-1, unchecked((ushort)address), true);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(-1);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return words.Found;
    }
}

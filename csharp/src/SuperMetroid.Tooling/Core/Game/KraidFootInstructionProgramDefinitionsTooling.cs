using SuperMetroid.Core.Hardware;
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
    /// <summary>Runs the shared layout generator selecting one mechanic word and no visuals.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(index, -1, false);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(-1);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return words.Selected;
    }
    /// <summary>Runs the shared layout generator selecting one visual word and no mechanics.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(-1, -1, false);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(index);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return presentation.Selected;
    }
    /// <summary>Uses the generated bank A7 layout to classify an address as mechanic data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000) return false;
        var words = new KraidFootInstructionProgramDefinitions.MechanicsSelection(-1, unchecked((ushort)address), true);
        var presentation = new KraidFootInstructionProgramDefinitions.PresentationSelection(-1);
        KraidFootInstructionProgramDefinitions.Generate(ref words, ref presentation);
        return words.Found;
    }
}

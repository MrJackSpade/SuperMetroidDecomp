using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Development-tool members of <see cref="KiHunterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterInstructionProgramDefinitions))]
internal abstract class KiHunterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Limits the Ki Hunter presentation operands enumerated by tooling.</summary>
    public static int PresentationWordCount => KiHunterInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand within the compiled Ki Hunter instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => KiHunterInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Supplies the native bank used when checking Ki Hunter instruction addresses.</summary>
    static int IDeclaredProgramBank.Bank => KiHunterInstructionProgramDefinitions.Bank;
    /// <summary>Limits mechanic words read from the compiled Ki Hunter layout.</summary>
    public static int MechanicsWordCount => KiHunterInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Exposes an addressed mechanic word for comparison with the native source.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = KiHunterInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Identifies bytes that the compiled layout owns as mechanic operands.</summary>
    public static bool IsCompiledMechanicsByte(int address) => KiHunterInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

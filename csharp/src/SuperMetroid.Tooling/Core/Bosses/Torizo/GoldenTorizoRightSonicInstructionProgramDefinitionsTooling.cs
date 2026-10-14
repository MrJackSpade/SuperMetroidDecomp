using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoRightSonicInstructionProgramDefinitions))]
internal abstract class GoldenTorizoRightSonicInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Bounds the right-sonic presentation operands inspected by the tooling catalog.</summary>
    public static int PresentationWordCount => GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one presentation operand in the compiled right-sonic instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the SNES bank against which instruction addresses are audited.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoRightSonicInstructionProgramDefinitions.Bank;
    /// <summary>Bounds the mechanic words exposed by the compiled right-sonic layout.</summary>
    public static int MechanicsWordCount => GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a mechanic word's ROM address with its compiled value for source comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes compiled mechanic bytes from presentation data during the audit.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

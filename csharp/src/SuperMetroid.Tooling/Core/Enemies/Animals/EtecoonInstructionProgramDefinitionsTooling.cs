using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EtecoonInstructionProgramDefinitions))]
internal abstract class EtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Bounds the Etecoon presentation operands inspected by the tooling catalog.</summary>
    public static int PresentationWordCount => EtecoonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one presentation operand in the compiled Etecoon instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => EtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the SNES bank against which instruction addresses are audited.</summary>
    static int IDeclaredProgramBank.Bank => EtecoonInstructionProgramDefinitions.Bank;
    /// <summary>Bounds the mechanic words exposed by the compiled Etecoon layout.</summary>
    public static int MechanicsWordCount => EtecoonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a mechanic word's ROM address with its compiled value for source comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = EtecoonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes compiled mechanic bytes from presentation data during the audit.</summary>
    public static bool IsCompiledMechanicsByte(int address) => EtecoonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

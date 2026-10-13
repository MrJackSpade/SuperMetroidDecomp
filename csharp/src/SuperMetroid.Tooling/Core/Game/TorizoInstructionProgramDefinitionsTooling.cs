using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoInstructionProgramDefinitions))]
internal abstract class TorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Bounds the Torizo presentation operands inspected by the tooling catalog.</summary>
    public static int PresentationWordCount => TorizoInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one presentation operand in the compiled Torizo instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the SNES bank against which instruction addresses are audited.</summary>
    static int IDeclaredProgramBank.Bank => TorizoInstructionProgramDefinitions.Bank;
    /// <summary>Bounds the mechanic words exposed by the compiled Torizo layout.</summary>
    public static int MechanicsWordCount => TorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a mechanic word's ROM address with its compiled value for source comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Distinguishes compiled mechanic bytes from presentation data during the audit.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

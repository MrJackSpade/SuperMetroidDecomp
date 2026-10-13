using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DachoraInstructionProgramDefinitions))]
internal abstract class DachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts Dachora instruction words whose presentation operands remain cartridge artwork data.</summary>
    public static int PresentationWordCount => DachoraInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a Dachora presentation operand for the tooling-side cartridge comparison.</summary>
    public static ushort PresentationWordAddress(int index) => DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the bank containing Dachora's compiled instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => DachoraInstructionProgramDefinitions.Bank;
    /// <summary>Counts the instruction words compiled as Dachora mechanics.</summary>
    public static int MechanicsWordCount => DachoraInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs a compiled Dachora mechanics word with its native address for auditing.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DachoraInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Identifies bytes covered by Dachora's compiled mechanics, excluding presentation data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DachoraInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

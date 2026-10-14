using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Development-tool members of <see cref="MaridiaLargeSnailInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MaridiaLargeSnailInstructionProgramDefinitions))]
internal abstract class MaridiaLargeSnailInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Supplies the native bank for interpreting large-snail instruction operands.</summary>
    static int IDeclaredProgramBank.Bank => MaridiaLargeSnailInstructionProgramDefinitions.Bank;
    /// <summary>Counts compiled large-snail control words exposed to development audits.</summary>
    public static int MechanicsWordCount => MaridiaLargeSnailInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Counts the large-snail layout's artwork operand slots.</summary>
    public static int PresentationWordCount => MaridiaLargeSnailInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Returns the native address and value for an indexed large-snail control word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MaridiaLargeSnailInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Maps an artwork operand index to its native instruction address.</summary>
    public static ushort PresentationWordAddress(int index) => MaridiaLargeSnailInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether a byte address belongs to compiled large-snail control data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MaridiaLargeSnailInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

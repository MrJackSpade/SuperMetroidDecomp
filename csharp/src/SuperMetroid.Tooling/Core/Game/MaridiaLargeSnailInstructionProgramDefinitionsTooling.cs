using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MaridiaLargeSnailInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MaridiaLargeSnailInstructionProgramDefinitions))]
internal abstract class MaridiaLargeSnailInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => MaridiaLargeSnailInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => MaridiaLargeSnailInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => MaridiaLargeSnailInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MaridiaLargeSnailInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => MaridiaLargeSnailInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => MaridiaLargeSnailInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

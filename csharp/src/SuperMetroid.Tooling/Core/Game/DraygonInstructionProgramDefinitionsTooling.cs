using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonInstructionProgramDefinitions))]
internal abstract class DraygonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => DraygonInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => DraygonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => DraygonInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DraygonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => DraygonInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => DraygonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

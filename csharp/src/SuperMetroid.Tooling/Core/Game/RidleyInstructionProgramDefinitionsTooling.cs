using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyInstructionProgramDefinitions))]
internal abstract class RidleyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => RidleyInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => RidleyInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => RidleyInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RidleyInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => RidleyInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => RidleyInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

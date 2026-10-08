using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireInstructionProgramDefinitions))]
internal abstract class CrocomireInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => CrocomireInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => CrocomireInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = CrocomireInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => CrocomireInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

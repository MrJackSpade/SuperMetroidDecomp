using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteInstructionProgramDefinitions))]
internal abstract class MagdolliteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => MagdolliteInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => MagdolliteInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MagdolliteInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => MagdolliteInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

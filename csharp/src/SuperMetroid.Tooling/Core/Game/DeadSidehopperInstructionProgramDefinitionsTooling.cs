using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadSidehopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadSidehopperInstructionProgramDefinitions))]
internal abstract class DeadSidehopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    static int IDeclaredProgramBank.Bank => DeadSidehopperInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static int PresentationWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    public static bool IsCompiledMechanicsByte(int address) => DeadSidehopperInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoFallingLeftInstructionProgramDefinitions))]
internal abstract class TorizoFallingLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static ushort PresentationWordAddress(int index) => TorizoFallingLeftInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => TorizoFallingLeftInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static int PresentationWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.PresentationSlotCount;
    public static bool IsCompiledMechanicsByte(int address) => TorizoFallingLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

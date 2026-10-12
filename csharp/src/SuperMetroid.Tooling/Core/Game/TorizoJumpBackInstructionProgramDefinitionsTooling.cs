using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoJumpBackInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoJumpBackInstructionProgramDefinitions))]
internal abstract class TorizoJumpBackInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => TorizoJumpBackInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => TorizoJumpBackInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => TorizoJumpBackInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => TorizoJumpBackInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoJumpBackInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => TorizoJumpBackInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

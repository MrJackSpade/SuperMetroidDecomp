using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoWalkingInstructionProgramDefinitions))]
internal abstract class GoldenTorizoWalkingInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => GoldenTorizoWalkingInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

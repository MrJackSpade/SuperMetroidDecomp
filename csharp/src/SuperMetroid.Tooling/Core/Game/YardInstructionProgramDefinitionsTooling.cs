using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YardInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YardInstructionProgramDefinitions))]
internal abstract class YardInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => YardInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => YardInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => YardInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => YardInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        (ushort address, ushort value) = YardInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => YardInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

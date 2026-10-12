using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoInstructionProgramDefinitions))]
internal abstract class TorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => TorizoInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => TorizoInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => TorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => TorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

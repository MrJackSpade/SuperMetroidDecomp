using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DachoraInstructionProgramDefinitions))]
internal abstract class DachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => DachoraInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => DachoraInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => DachoraInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DachoraInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => DachoraInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

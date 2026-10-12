using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EtecoonInstructionProgramDefinitions))]
internal abstract class EtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => EtecoonInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => EtecoonInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => EtecoonInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = EtecoonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => EtecoonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

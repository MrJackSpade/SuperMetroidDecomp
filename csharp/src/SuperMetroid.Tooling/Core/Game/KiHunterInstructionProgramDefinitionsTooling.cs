using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KiHunterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterInstructionProgramDefinitions))]
internal abstract class KiHunterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => KiHunterInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => KiHunterInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => KiHunterInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => KiHunterInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = KiHunterInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => KiHunterInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

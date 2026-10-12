using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftFootOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

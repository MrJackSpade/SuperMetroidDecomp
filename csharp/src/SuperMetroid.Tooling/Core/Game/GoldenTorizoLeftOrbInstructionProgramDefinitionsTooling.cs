using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftOrbInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

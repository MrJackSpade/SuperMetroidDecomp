using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoLandingDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoLandingDustInstructionProgramDefinitions))]
internal abstract class TorizoLandingDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => TorizoLandingDustInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => TorizoLandingDustInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainTopTubeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTopTubeInstructionProgramDefinitions))]
internal abstract class MotherBrainTopTubeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => MotherBrainTopTubeInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => MotherBrainTopTubeInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}

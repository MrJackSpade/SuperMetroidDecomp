using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawInstructionProgramDefinitions))]
internal abstract class YappingMawInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => YappingMawInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => YappingMawInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => YappingMawInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = YappingMawInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}

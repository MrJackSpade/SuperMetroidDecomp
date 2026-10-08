using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WalkingSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WalkingSpacePirateInstructionProgramDefinitions))]
internal abstract class WalkingSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}

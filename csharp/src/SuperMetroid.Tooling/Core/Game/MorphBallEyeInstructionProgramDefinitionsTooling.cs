using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MorphBallEyeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MorphBallEyeInstructionProgramDefinitions))]
internal abstract class MorphBallEyeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

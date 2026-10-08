using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SparkInstructionProgramDefinitions))]
internal abstract class SparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => SparkInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => SparkInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => SparkInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => SparkInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SparkInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SparkInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ZeroInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZeroInstructionProgramDefinitions))]
internal abstract class ZeroInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ZeroInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ZeroInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ZeroInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ZeroInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ZeroInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ZeroInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EvirInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EvirInstructionProgramDefinitions))]
internal abstract class EvirInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => EvirInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => EvirInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => EvirInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => EvirInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EvirInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EvirInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

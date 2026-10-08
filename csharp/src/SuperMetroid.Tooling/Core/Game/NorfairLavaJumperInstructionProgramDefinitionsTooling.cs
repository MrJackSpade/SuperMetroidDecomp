using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairLavaJumperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairLavaJumperInstructionProgramDefinitions))]
internal abstract class NorfairLavaJumperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairLavaJumperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairLavaJumperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}

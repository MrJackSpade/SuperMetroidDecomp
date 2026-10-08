using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PowampInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PowampInstructionProgramDefinitions))]
internal abstract class PowampInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => PowampInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => PowampInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => PowampInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => PowampInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PowampInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PowampInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

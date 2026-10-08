using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SbugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SbugInstructionProgramDefinitions))]
internal abstract class SbugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => SbugInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => SbugInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => SbugInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => SbugInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SbugInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SbugInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

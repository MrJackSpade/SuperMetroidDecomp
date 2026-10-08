using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkulteraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkulteraInstructionProgramDefinitions))]
internal abstract class SkulteraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => SkulteraInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => SkulteraInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => SkulteraInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SkulteraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SkulteraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

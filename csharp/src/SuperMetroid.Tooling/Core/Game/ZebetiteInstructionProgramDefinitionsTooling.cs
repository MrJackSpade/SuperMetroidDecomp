using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ZebetiteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ZebetiteInstructionProgramDefinitions))]
internal abstract class ZebetiteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ZebetiteInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ZebetiteInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ZebetiteInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ZebetiteInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ZebetiteInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ZebetiteInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

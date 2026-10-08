using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RioInstructionProgramDefinitions))]
internal abstract class RioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => RioInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => RioInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => RioInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => RioInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < RioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = RioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

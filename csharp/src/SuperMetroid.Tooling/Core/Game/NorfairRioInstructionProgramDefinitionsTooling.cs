using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NorfairRioInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NorfairRioInstructionProgramDefinitions))]
internal abstract class NorfairRioInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => NorfairRioInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NorfairRioInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}

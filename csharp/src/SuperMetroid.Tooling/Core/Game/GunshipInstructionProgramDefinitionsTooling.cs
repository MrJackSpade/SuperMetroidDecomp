using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GunshipInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GunshipInstructionProgramDefinitions))]
internal abstract class GunshipInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => GunshipInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => GunshipInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => GunshipInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GunshipInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = GunshipInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

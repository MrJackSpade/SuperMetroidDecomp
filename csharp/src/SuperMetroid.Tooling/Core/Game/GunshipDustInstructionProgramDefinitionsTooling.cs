using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GunshipDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GunshipDustInstructionProgramDefinitions))]
internal abstract class GunshipDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => GunshipDustInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => GunshipDustInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => GunshipDustInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GunshipDustInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = GunshipDustInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

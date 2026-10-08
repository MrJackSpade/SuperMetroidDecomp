using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawBodyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawBodyProjectileInstructionProgramDefinitions))]
internal abstract class YappingMawBodyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

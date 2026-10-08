using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KiHunterAcidSpitInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterAcidSpitInstructionProgramDefinitions))]
internal abstract class KiHunterAcidSpitInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

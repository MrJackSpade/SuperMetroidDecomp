using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DownwardGateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DownwardGateProjectileInstructionProgramDefinitions))]
internal abstract class DownwardGateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteLavaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteLavaInstructionProgramDefinitions))]
internal abstract class MagdolliteLavaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

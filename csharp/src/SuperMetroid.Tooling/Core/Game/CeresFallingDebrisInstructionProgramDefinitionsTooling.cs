using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresFallingDebrisInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresFallingDebrisInstructionProgramDefinitions))]
internal abstract class CeresFallingDebrisInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

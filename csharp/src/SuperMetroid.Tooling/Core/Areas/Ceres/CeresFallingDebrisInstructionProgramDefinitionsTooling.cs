using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Development-tool members of <see cref="CeresFallingDebrisInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresFallingDebrisInstructionProgramDefinitions))]
internal abstract class CeresFallingDebrisInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Ceres falling-debris instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled debris mechanics word with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts debris presentation operands retained as visual data.</summary>
    public static int PresentationWordCount => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Ceres falling-debris presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled debris words in the enemy-projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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

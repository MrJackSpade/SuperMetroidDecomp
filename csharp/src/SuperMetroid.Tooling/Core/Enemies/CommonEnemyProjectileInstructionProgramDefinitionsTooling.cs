using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies;

/// <summary>Development-tool members of <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CommonEnemyProjectileInstructionProgramDefinitions))]
internal abstract class CommonEnemyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The shared delete opcode is the catalog's only compiled mechanics word.</summary>
    public static int MechanicsWordCount => 1;
    /// <summary>Maps the sole catalog entry to the shared enemy-projectile delete instruction.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => index == 0
        ? new(CommonEnemyProjectileInstructionProgramDefinitions.Delete, (ushort)EnemyProjectileInstruction.Delete)
        : throw new IndexOutOfRangeException();
    /// <summary>Recognizes either byte of that delete opcode only in the enemy-projectile code bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is CommonEnemyProjectileInstructionProgramDefinitions.Delete or
            ((ushort)(CommonEnemyProjectileInstructionProgramDefinitions.Delete + 1));
    }
}

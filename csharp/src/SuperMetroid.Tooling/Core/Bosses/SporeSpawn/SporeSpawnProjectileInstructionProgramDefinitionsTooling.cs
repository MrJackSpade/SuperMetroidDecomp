using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Development-tool members of <see cref="SporeSpawnProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SporeSpawnProjectileInstructionProgramDefinitions))]
internal abstract class SporeSpawnProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Spore Spawn projectile mechanic words available to source audits.</summary>
    public static int MechanicsWordCount => SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Retrieves an addressed mechanic word from the compiled projectile program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds visual operands separately from the projectile's mechanic words.</summary>
    public static int PresentationWordCount => SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand for comparison with the native program.</summary>
    public static ushort PresentationWordAddress(int index) => SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a mechanic word in the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

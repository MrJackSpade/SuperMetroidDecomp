using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="TorizoExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosionInstructionProgramDefinitions))]
internal abstract class TorizoExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the explosion's mechanic words exposed to source audits.</summary>
    public static int MechanicsWordCount => TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Retrieves an addressed mechanic word from the compiled explosion program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds visual operands separately from mechanic instruction words.</summary>
    public static int PresentationWordCount => TorizoExplosionInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one explosion presentation operand in the native stream.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches both bytes of each mechanic word inside the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

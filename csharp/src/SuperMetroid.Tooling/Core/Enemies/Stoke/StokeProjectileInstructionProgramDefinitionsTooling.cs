using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Development-tool members of <see cref="StokeProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(StokeProjectileInstructionProgramDefinitions))]
internal abstract class StokeProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in Stoke's projectile program.</summary>
    public static int MechanicsWordCount => StokeProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a compiled Stoke projectile word from the extracted program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => StokeProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in Stoke's projectile animation.</summary>
    public static int PresentationWordCount => StokeProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an extracted Stoke projectile drawing operand.</summary>
    public static ushort PresentationWordAddress(int index) => StokeProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled Stoke projectile word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < StokeProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = StokeProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

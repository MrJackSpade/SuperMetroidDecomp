using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Development-tool members of <see cref="PhantoonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonProjectileInstructionProgramDefinitions))]
internal abstract class PhantoonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled control words in Phantoon's projectile animation programs.</summary>
    public static int MechanicsWordCount => PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes an indexed Phantoon projectile timing or instruction word for native-data checks.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts the spritemap operands interleaved with Phantoon projectile control.</summary>
    public static int PresentationWordCount => PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an artwork selection in a Phantoon projectile list.</summary>
    public static ushort PresentationWordAddress(int index) => PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a bank-$86 Phantoon projectile control word, excluding spritemap operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

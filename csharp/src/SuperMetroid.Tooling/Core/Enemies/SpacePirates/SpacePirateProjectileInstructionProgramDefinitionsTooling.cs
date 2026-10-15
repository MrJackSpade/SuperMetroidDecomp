using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Development-tool members of <see cref="SpacePirateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SpacePirateProjectileInstructionProgramDefinitions))]
internal abstract class SpacePirateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Timing and control words in the pirate laser and claw lists for both directions.</summary>
    public static int MechanicsWordCount => SpacePirateProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads one compiled word from a laser or claw projectile list.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SpacePirateProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands for the left and right laser and claw animations.</summary>
    public static int PresentationWordCount => SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a laser or claw projectile drawing operand.</summary>
    public static ushort PresentationWordAddress(int index) => SpacePirateProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled pirate projectile word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SpacePirateProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SpacePirateProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Development-tool members of <see cref="KiHunterAcidSpitInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterAcidSpitInstructionProgramDefinitions))]
internal abstract class KiHunterAcidSpitInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in the Ki Hunter acid spit programs.</summary>
    public static int MechanicsWordCount => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads an acid spit instruction word from the extracted mechanics catalog.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in the Ki Hunter acid spit programs.</summary>
    public static int PresentationWordCount => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an acid spit drawing operand by catalog index.</summary>
    public static ushort PresentationWordAddress(int index) => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of an acid spit mechanics word in the projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}

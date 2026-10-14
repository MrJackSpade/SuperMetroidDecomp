using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Development-tool members of <see cref="GunshipDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GunshipDustInstructionProgramDefinitions))]
internal abstract class GunshipDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in the gunship dust projectile lists.</summary>
    public static int MechanicsWordCount => GunshipDustInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a word from the extracted gunship dust mechanics catalog.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => GunshipDustInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in the gunship dust projectile lists.</summary>
    public static int PresentationWordCount => GunshipDustInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an extracted gunship dust drawing operand.</summary>
    public static ushort PresentationWordAddress(int index) => GunshipDustInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled dust word in the projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = GunshipDustInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

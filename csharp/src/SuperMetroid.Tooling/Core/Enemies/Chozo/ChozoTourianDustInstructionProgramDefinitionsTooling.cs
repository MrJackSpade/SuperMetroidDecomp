using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Development-tool members of <see cref="ChozoTourianDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoTourianDustInstructionProgramDefinitions))]
internal abstract class ChozoTourianDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The compiled timing and control words in the Chozo and Tourian dust catalog.</summary>
    public static int MechanicsWordCount => ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads an extracted dust instruction mechanics word by catalog index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The dust programs' drawing operand count for dependency auditing.</summary>
    public static int PresentationWordCount => ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an extracted dust drawing operand by catalog index.</summary>
    public static ushort PresentationWordAddress(int index) => ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each cataloged word in the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address ||
                bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

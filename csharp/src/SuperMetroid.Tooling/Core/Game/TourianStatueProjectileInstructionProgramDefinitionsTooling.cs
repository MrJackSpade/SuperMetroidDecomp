using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TourianStatueProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianStatueProjectileInstructionProgramDefinitions))]
internal abstract class TourianStatueProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Timing and control words for the Tourian statue projectile lists.</summary>
    public static int MechanicsWordCount => TourianStatueProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads one compiled statue projectile word from the extracted catalog.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => TourianStatueProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands used by the Tourian statue projectiles.</summary>
    public static int PresentationWordCount => TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a statue projectile drawing operand by catalog index.</summary>
    public static ushort PresentationWordAddress(int index) => TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a statue projectile mechanics word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TourianStatueProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TourianStatueProjectileInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

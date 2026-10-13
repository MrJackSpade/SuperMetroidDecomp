using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEggInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEggInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Golden Torizo egg instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled egg-program mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Golden Torizo egg presentation words retained as visual operands.</summary>
    public static int PresentationWordCount => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Golden Torizo egg presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Includes the shared Chozo-orb wall-impact span and egg words in the enemy-projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if (TorizoChozoOrbInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) &&
            unchecked((ushort)address) is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab40)
        {
            return true;
        }
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

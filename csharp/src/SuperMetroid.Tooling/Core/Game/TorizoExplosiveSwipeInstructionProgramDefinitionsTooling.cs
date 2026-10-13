using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoExplosiveSwipeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosiveSwipeInstructionProgramDefinitions))]
internal abstract class TorizoExplosiveSwipeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled mechanics words in Torizo's explosive swipe projectile program.</summary>
    public static int MechanicsWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns an explosive swipe mechanics word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts explosive swipe presentation words retained as visual data.</summary>
    public static int PresentationWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed explosive swipe presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled swipe words in the enemy-projectile code bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

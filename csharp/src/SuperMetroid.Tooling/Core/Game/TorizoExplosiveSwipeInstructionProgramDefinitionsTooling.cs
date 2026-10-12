using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoExplosiveSwipeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosiveSwipeInstructionProgramDefinitions))]
internal abstract class TorizoExplosiveSwipeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress(index);
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

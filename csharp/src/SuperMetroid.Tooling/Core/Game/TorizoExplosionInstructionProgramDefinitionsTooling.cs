using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosionInstructionProgramDefinitions))]
internal abstract class TorizoExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => TorizoExplosionInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}

using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChozoTourianDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoTourianDustInstructionProgramDefinitions))]
internal abstract class ChozoTourianDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
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

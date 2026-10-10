using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CommonEnemyProjectileInstructionProgramDefinitions))]
internal abstract class CommonEnemyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 1;
    public static InstructionMechanicsWord MechanicsWord(int index) => index == 0
        ? new(CommonEnemyProjectileInstructionProgramDefinitions.Delete, (ushort)EnemyProjectileInstruction.Delete)
        : throw new IndexOutOfRangeException();
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is CommonEnemyProjectileInstructionProgramDefinitions.Delete or
            ((ushort)(CommonEnemyProjectileInstructionProgramDefinitions.Delete + 1));
    }
}

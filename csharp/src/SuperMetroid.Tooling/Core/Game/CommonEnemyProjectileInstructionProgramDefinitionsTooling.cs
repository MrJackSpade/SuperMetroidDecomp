using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CommonEnemyProjectileInstructionProgramDefinitions))]
internal abstract class CommonEnemyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 1;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => index == 0
        ? new(CommonEnemyProjectileInstructionProgramDefinitions.Delete, (ushort)EnemyProjectileInstruction.Delete)
        : throw new IndexOutOfRangeException();
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is CommonEnemyProjectileInstructionProgramDefinitions.Delete or
            ((ushort)(CommonEnemyProjectileInstructionProgramDefinitions.Delete + 1));
    }
}

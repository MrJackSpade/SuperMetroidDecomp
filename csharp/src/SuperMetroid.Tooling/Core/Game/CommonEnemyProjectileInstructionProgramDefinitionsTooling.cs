using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CommonEnemyProjectileInstructionProgramDefinitions))]
internal abstract class CommonEnemyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>This projectile program contains only the native delete instruction as an editable mechanics word.</summary>
    public static int MechanicsWordCount => 1;

    /// <summary>Returns the address and value of the program's delete instruction.</summary>
    /// <param name="index">Zero-based mechanics-word index; only index zero is defined.</param>
    /// <returns>The delete instruction's native address and opcode.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is not zero.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => index == 0
        ? new(CommonEnemyProjectileInstructionProgramDefinitions.Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete)
        : throw new IndexOutOfRangeException();

    /// <summary>Reports whether an absolute address selects either byte of the bank-$B4 delete instruction word.</summary>
    /// <param name="address">Absolute address to test, including its bank component.</param>
    /// <returns><see langword="true"/> only for the two bytes of the compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress == CommonEnemyProjectileInstructionProgramDefinitions.Delete ||
            bankAddress == unchecked((ushort)(CommonEnemyProjectileInstructionProgramDefinitions.Delete + 1));
    }
}

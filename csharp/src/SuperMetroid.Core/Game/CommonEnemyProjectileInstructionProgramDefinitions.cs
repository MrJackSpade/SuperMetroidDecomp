namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for bank-$86 instruction programs shared by otherwise independent
/// projectile families. These programs are checked before a projectile's private owner;
/// family catalogs therefore remain strict without duplicating native shared targets.
/// </summary>
internal abstract class CommonEnemyProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_Delete</c> at $86:84FC.</summary>
    internal const ushort Delete = 0x84fc;

    public static int MechanicsWordCount => 1;
    public static InstructionMechanicsWord MechanicsWord(int index) => index == 0
        ? new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete)
        : throw new IndexOutOfRangeException();
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == Delete)
        {
            value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
            return true;
        }

        value = 0;
        return false;
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress == Delete ||
            bankAddress == unchecked((ushort)(Delete + 1));
    }
}

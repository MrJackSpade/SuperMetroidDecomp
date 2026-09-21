namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics word in a shared bank-$86 projectile program.</summary>
internal readonly record struct CommonEnemyProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for bank-$86 instruction programs shared by otherwise independent
/// projectile families. These programs are checked before a projectile's private owner;
/// family catalogs therefore remain strict without duplicating native shared targets.
/// </summary>
internal static class CommonEnemyProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Delete</c> at $86:84FC.</summary>
    internal const ushort Delete = 0x84fc;

    private static readonly CommonEnemyProjectileInstructionMechanicsWord[] Words =
    [
        new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static CommonEnemyProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];

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

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryReadMechanicsWord(address, out ushort value))
            return value;

        throw new InvalidDataException(
            $"Shared enemy-projectile instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress == Delete ||
            bankAddress == unchecked((ushort)(Delete + 1));
    }
}

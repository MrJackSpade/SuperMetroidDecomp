namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for bank-$86 instruction programs shared by otherwise independent
/// projectile families. These programs are checked before a projectile's private owner;
/// family catalogs therefore remain strict without duplicating native shared targets.
/// </summary>
internal abstract class CommonEnemyProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Delete</c> at $86:84FC.</summary>
    internal const ushort Delete = 0x84fc;

    /// <summary>Resolves the shared bank-$86 projectile-delete instruction to its compiled instruction pointer.</summary>
    /// <param name="address">Bank-local instruction address to check.</param>
    /// <param name="value">Receives the compiled delete instruction pointer when this definition owns the address.</param>
    /// <returns><see langword="true"/> only for <see cref="Delete"/>; otherwise, <see langword="false"/>.</returns>
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
}

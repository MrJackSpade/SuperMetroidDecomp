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
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == Delete)
        {
            value = (ushort)EnemyProjectileInstruction.Delete;
            return true;
        }

        value = 0;
        return false;
    }
}

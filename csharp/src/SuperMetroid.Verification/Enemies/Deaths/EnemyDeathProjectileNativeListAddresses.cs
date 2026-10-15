using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>
/// The native bank-$86 lists the enemy death and drop ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EnemyDeathProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (EnemyDeathProjectileProgram Program, ushort Entry)[] Entries =
    [
        (EnemyDeathProjectileProgram.RespawnTail, 0xeca3),
        (EnemyDeathProjectileProgram.BigExplosion, 0xecab),
        (EnemyDeathProjectileProgram.BigExplosionLoop, 0xecaf),
        (EnemyDeathProjectileProgram.MiniKraidExplosion, 0xecc5),
        (EnemyDeathProjectileProgram.MiniKraidExplosionLoop, 0xecc9),
        (EnemyDeathProjectileProgram.NormalExplosion, 0xed4b),
        (EnemyDeathProjectileProgram.SmallExplosion, 0xed69),
        (EnemyDeathProjectileProgram.SmallEnergy, 0xed8d),
        (EnemyDeathProjectileProgram.BigEnergy, 0xeda3),
        (EnemyDeathProjectileProgram.Missiles, 0xedb9),
        (EnemyDeathProjectileProgram.SuperMissiles, 0xeddd),
        (EnemyDeathProjectileProgram.PowerBombs, 0xedeb),
        (EnemyDeathProjectileProgram.KilledBySamusContact, 0xedff),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:ECA3-EE42, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xeca3, 0xee43);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EnemyDeathProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
